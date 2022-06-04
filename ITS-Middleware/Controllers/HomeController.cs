using System.Data.Entity.Infrastructure;
using ITS_Middleware.Models;
using ITS_Middleware.Models.Entities;
using ITS_Middleware.Tools;
using Microsoft.AspNetCore.Mvc;


namespace ITS_Middleware.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public MiddlewareDbContext _context;

        public HomeController(MiddlewareDbContext master, ILogger<HomeController> logger)
        {
            _context = master;
            _logger = logger;
        }


        public IActionResult Projects()
        {
            try
            {
                ViewBag.email = HttpContext.Session.GetString("userEmail");
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString().Trim());
                return Json("Error");
            }
        }


        [HttpGet] //Get all Users
        public IActionResult Users()
        {
            try
            {
                var data = _context.Usuarios.OrderBy(u => u.Id).ToList();
                ViewBag.email = HttpContext.Session.GetString("userEmail");
                return View(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString().Trim());
                return Json("Error");
            }
        }

         public IActionResult RegisterUser()
        {
            try 
            {
                return View();
            } 
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message.ToString());
                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([Bind("Nombre,FechaAlta,Puesto,Email,Pass")] Usuario user)
        {   
            try
            {      
                if (ModelState.IsValid)
                {
                    var passHashed = Encrypt.GetSHA256(user.Pass);
                    user.Pass = passHashed;
                    _context.Add(user);
                    
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Users", "Home");
                }
                return Json(user);
            }
            catch (Exception ex)
            {
                string value = ex.Message.ToString();
                Console.Write(value); 
            }
            return View(user);
        }


         //Editar usuario
        public async Task<IActionResult> EditUser(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(int id, [Bind("id,nombre,pass,fechaAlta,puesto,email")] Usuario user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                try
                {
                    var cifra = Encrypt.GetSHA256(user.Pass);
                    user.Pass = cifra;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExiste(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Users", "Home");
            }
            return View(user);
        }



        //Eliminar usuario
        public async Task<IActionResult> DeleteUser(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }


        
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {

            var user = await _context.Usuarios.FindAsync(id);

            _context.Usuarios.Remove(user);

            await _context.SaveChangesAsync();
            return RedirectToAction("Users", "Home");
          
        }



        private bool UsuarioExiste(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}