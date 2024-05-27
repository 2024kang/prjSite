using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using prjSite2.Models;
using System.Text;

namespace prjSite2.Controllers
{
    public class HWController : Controller
    {
     
        public IActionResult HW2()
        {        
            return View();
        }
        public IActionResult HW1()
        {
            return View();
        }

        public IActionResult HW3()
        {
            return View();
        }

       
        public IActionResult HW4() 
        {
            return View();
        }
    }
}
