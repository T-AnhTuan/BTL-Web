using Microsoft.Extensions.Options;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System.Text;
using System.Text.Json;

namespace QuanLyVatTu.Services
{
    public interface INhatKyService
    {
        Task GhiNhatKyAsync(NhatKyHeThong nhatKy);
        Task GhiNhatKyAsync(int taiKhoanId, string hanhDong, string diaChiIp = null);
        Task GhiVaDayLenServerAsync(NhatKyHeThong nhatKy);
    }
    public class NhatKyOptions
    {
        public string ServerUrl { get; set; } // endpoint để đẩy nhật ký
        public string ApiKey { get; set; } // nếu cần auth
        public bool EnableRemotePush { get; set; } = false;
    }


    public class NhatKyService : INhatKyService
    {

        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NhatKyOptions _options;
        private readonly ILogger<NhatKyService> _logger;

        public NhatKyService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IOptions<NhatKyOptions> options,
            ILogger<NhatKyService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task GhiNhatKyAsync(NhatKyHeThong nhatKy)
        {
            if (nhatKy == null) throw new ArgumentNullException(nameof(nhatKy));

            try
            {
                nhatKy.ThoiGian = nhatKy.ThoiGian == default ? DateTime.Now : nhatKy.ThoiGian;
                _context.NhatKyHeThongs.Add(nhatKy);
                await _context.SaveChangesAsync();

                if (_options.EnableRemotePush)
                {
                    // không chặn luồng chính nếu push thất bại — gọi bất đồng bộ
                    _ = GhiVaDayLenServerAsync(nhatKy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi nhật ký vào DB");
                throw;
            }
        }

        public Task GhiNhatKyAsync(int taiKhoanId, string hanhDong, string diaChiIp = null)
        {
            var entry = new NhatKyHeThong
            {
                TaiKhoanId = taiKhoanId,
                HanhDong = hanhDong,
                DiaChiIP = diaChiIp,
                ThoiGian = DateTime.Now
            };
            return GhiNhatKyAsync(entry);
        }

        public async Task GhiVaDayLenServerAsync(NhatKyHeThong nhatKy)
        {
            if (!_options.EnableRemotePush)
            {
                _logger.LogDebug("Remote push disabled, bỏ qua đẩy lên server.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.ServerUrl))
            {
                _logger.LogWarning("ServerUrl chưa cấu hình, bỏ qua đẩy lên server.");
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("NhatKyClient");
                var payload = new
                {
                    nhatKy.Id,
                    nhatKy.TaiKhoanId,
                    nhatKy.HanhDong,
                    ThoiGian = nhatKy.ThoiGian.ToString("o"),
                    nhatKy.DiaChiIP
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(_options.ApiKey))
                {
                    client.DefaultRequestHeaders.Remove("X-Api-Key");
                    client.DefaultRequestHeaders.Add("X-Api-Key", _options.ApiKey);
                }

                var resp = await client.PostAsync(_options.ServerUrl, content);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Đẩy nhật ký lên server trả về {Status}: {Body}", resp.StatusCode, body);
                }
                else
                {
                    _logger.LogDebug("Đẩy nhật ký lên server thành công. Id={Id}", nhatKy.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đẩy nhật ký lên server");
                // Không ném tiếp để tránh ảnh hưởng luồng chính; có thể queue lại hoặc retry tuỳ nhu cầu.
            }
        }
    }
}
