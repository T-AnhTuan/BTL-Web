using QuanLyVatTu.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly AppDbContext _context;
        public DanhMucController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> VatTu()
        {
            try
            {
                var data = await _context.DanhMucVatTus.ToListAsync();
                return View(data);
            }
            catch (Exception ex) // Bắt lỗi
            {
                Console.WriteLine("Lỗi load danh mục: " + ex.Message);
                return View();
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> NhaCungCap()
        {
            try
            {
                var data = await _context.NhaCungCaps.ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi load danh mục: " + ex.Message);
                return View();
            }
        }
       
    }
}