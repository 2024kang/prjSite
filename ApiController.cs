using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MSIT158Site.Models.DTO;
using prjSite2.Models;
using prjSite2.Models.DTO;
using System.Text;
using Microsoft.CodeAnalysis;
using System.IO;

namespace prjSite2.Controllers
{
    public class ApiController : Controller
    {
        private readonly MyDBContext _context;
        //實際路徑應該要動態的進行改變，再apicontroller中透過注入的方式
        //因為是注入在建構子裡面，所以uploadPath會用不到，所以要寫成全域變數
        private readonly IWebHostEnvironment _hostEnviroment;

        public ApiController(MyDBContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnviroment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return Content("Hello World","text/plain", Encoding.UTF8);
        }
        //台灣各縣市
        public IActionResult Json()
        {
            var cities = _context.Addresses.Select(x => x.City).Distinct();

            return Json(cities);
        }
 

        public IActionResult Avatar(int id =1) {

            Member? member = _context.Members.Find(id);

            if (member != null) {

                byte[] img = member.FileData;

                if (img != null)
                {
                    return File(img, "image/jpeg");
                }
            
            }

            return NotFound();
        }


        //讀出不會重複的城市名
        public IActionResult Cities()
        {
            var cities = _context.Addresses.Select(a => a.City).Distinct();
            return Json(cities);
        }

        //根據城市名讀出不會重複的鄉鎮區
        public IActionResult Districts(string city)
        {
            var districts = _context.Addresses.Where(a => a.City == city).Select(a => a.SiteId).Distinct();
            return Json(districts);
        }
        //根據鄉鎮區讀出路名
        public IActionResult Roads(string districts)
        {
            var roads = _context.Addresses.Where(a => a.SiteId == districts).Select(a => a.Road);
            return Json(roads);
        }

        public IActionResult first()
        {
            Thread.Sleep(10000);
            return View();
        }

       

        public IActionResult homework2()
        {

            return View();

        }

        public IActionResult Register(Member member, IFormFile avatar)
        {
            if (string.IsNullOrEmpty(member.Name))
            {
                member.Name = "guest";
            }
            //string info = $"{avatar.FileName} - {avatar.Length} - {avatar.ContentType}";
            //string info = _iwhe.ContentRootPath;
            //檔案上傳-->實際路徑
            //string uploadPath = @"C:\Users\User\Documents\workspace\MSIT158Site\wwwroot\uploads\a.png";
            //WebRootPath：C:\Users\User\Documents\workspace\MSIT158Site\wwwroot
            //ContentRootPath：C:\Users\User\Documents\workspace\MSIT158Site
            // 組合檔案上傳路徑
            string uploadPath = Path.Combine(_hostEnviroment.WebRootPath, "uploads", avatar.FileName);

            // 檢查並創建目錄
            if (!Directory.Exists(uploadPath))
            { 
                Directory.CreateDirectory(uploadPath);
            }

            // 檔案名稱安全檢查
            string fileName = Path.GetFileName(avatar.FileName);
            string filePath = Path.Combine(uploadPath, fileName);


            
            // 初始化 imgByte 為空
            byte[]? imgByte = null;
            //寫進資料庫
            using (var memoryString = new MemoryStream())
            {
                avatar.CopyTo(memoryString);
                imgByte = memoryString.ToArray();

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    memoryString.Position = 0; // 重設 memoryStream 位置
                    memoryString.CopyTo(fileStream);
                }
            }

            member.FileName = fileName;
            member.FileData = imgByte;

            _context.Members.Add(member);
            _context.SaveChanges();
            //return Content($"Hello {member.Name}. You're over {member.Age}. E-mail: {member.Email}");
  
            return Content($"{filePath}", "text/plain", System.Text.Encoding.UTF8);
        }

        public IActionResult CheckAccount(string name)
        {
            var member = _context.Members.Any(m => m.Name == name);

            return Content(member.ToString(), "text/plain", System.Text.Encoding.UTF8);
        }





        [HttpPost]
        public IActionResult Spots([FromBody] CSearchDTO searchDTO)
        {
            //根據分類編號搜尋景點資料
            var spots = searchDTO.categoryId == 0 ? _context.SpotImagesSpots : _context.SpotImagesSpots.Where(s => s.CategoryId == searchDTO.categoryId);

            //根據關鍵字搜尋景點資料(title、desc)
            if (!string.IsNullOrEmpty(searchDTO.keyword))
            {
                spots = spots.Where(s => s.SpotTitle.Contains(searchDTO.keyword) || s.SpotDescription.Contains(searchDTO.keyword));
            }

            //排序
            switch (searchDTO.sortBy)
            {
                case "spotTitle":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
                    break;
                case "categoryId":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
                    break;
                default:
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                    break;
            }





            //總共有多少筆資料
            int totalCount = spots.Count();
            //每頁要顯示幾筆資料
            int pageSize = searchDTO.pageSize;   //searchDTO.pageSize ?? 9;
            //目前第幾頁
            int page = searchDTO.page;

            //計算總共有幾頁
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);

            //分頁
            spots = spots.Skip((page - 1) * pageSize).Take(pageSize);


            //包裝要傳給client端的資料
            CSpotsPagingDTO spotsPaging = new CSpotsPagingDTO();
            spotsPaging.TotalCount = totalCount;
            spotsPaging.TotalPages = totalPages;
           //改 spotsPaging.SpotsResult = sspots.ToList();


            return Json(spotsPaging);
        }


    }
}

