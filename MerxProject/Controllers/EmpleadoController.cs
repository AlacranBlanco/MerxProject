using MerxProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    public class EmpleadoController : Controller
    {
        private ApplicationUserManager _userManager;

        private readonly int _RegistrosPorPagina = 10;

        private List<Empleado> _Empleados;
        private PaginadorGenerico<Empleado> _PaginadorEmpleado;
        

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public ActionResult PopUpEmpleados(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (accion == "1")
                {
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    return View();
                }
                else if (accion == "2")
                {
                    var empleado = DbModel.Empleados.Find(Id);
                    empleado.Usuarioss = (Usuario)empleado.Usuarioss;
                    empleado.Personass = (Persona)empleado.Personass;
                    ViewBag.pass = empleado.Usuarioss.Password;
                    ViewBag.puesto = empleado.Usuarioss.Rol;
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    return View(empleado);
                }
                else if (accion == "3")
                {
                    var empleado = DbModel.Empleados.Find(Id);
                    empleado.Usuarioss = (Usuario)empleado.Usuarioss;
                    empleado.Personass = (Persona)empleado.Personass;
                    ViewBag.pass = empleado.Usuarioss.Password;
                    ViewBag.puesto = empleado.Usuarioss.Rol;
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    return View(empleado);
                }
            }
            return RedirectToAction("ListaEmpleado");
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult> PopUpEmpleados(Empleado empleados, string accion, string rol, string password)
        {
            string res;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usuario quiere Editar
                if (empleados.Id > 0 && accion == "2")
                {
                    // Edición
                    var empleado = DbModel.Empleados.Find(empleados.Id);

                    if (empleado != null)
                    {
                        try
                        {
                            empleados.FechaIngreso = DateTime.Now;
                            empleados.Usuarioss.Rol = rol;
                            empleados.Usuarioss.Password = password;


                            if (empleados.Personass.Correo != empleado.Personass.Correo && empleados.Usuarioss.Rol == rol)
                            {
                                var userDel = DbModel.Users.Where(x => x.idPersona == empleado.Personass.idPersona).FirstOrDefault();
                                var userName = userDel.UserName;
                                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbModel));
                                var rolDel = RoleManager.FindByName(empleados.Usuarioss.Rol).Name;

                                UserManager.RemoveFromRole(userName, rolDel);
                                DbModel.Users.Remove(userDel);

                                var user = new ApplicationUser { UserName = empleados.Usuarioss.User, Email = empleados.Personass.Correo, idPersona = empleados.Personass.idPersona, IdUsuario = empleados.Usuarioss.idUsuario };
                                var result = await UserManager.CreateAsync(user, empleados.Usuarioss.Password);
                                if (result.Succeeded)
                                {
                                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                                    await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", callbackUrl);

                                    DbModel.Empleados.AddOrUpdate(empleados);
                                    DbModel.Usuarios.AddOrUpdate(empleados.Usuarioss);
                                    DbModel.Personas.AddOrUpdate(empleados.Personass);
                                    DbModel.SaveChanges();

                                    var UsuarioNuevo = UserManager.FindByEmail(empleados.Personass.Correo).Id;
                                    UserManager.AddToRole(UsuarioNuevo, rolDel);

                                    res = "Actualización realizada";
                                    Session["res"] = res;
                                    Session["tipo"] = "Exito";
                                    return RedirectToAction("ListaEmpleado");
                                }
                                else
                                {
                                    Session["res"] = result.Errors.ToList();
                                    return RedirectToAction("ListaEmpleado");
                                }
                            }
                            else if (empleados.Personass.Correo == empleado.Personass.Correo && empleados.Usuarioss.Rol != empleado.Usuarioss.Rol)
                            {
                                var userDel = DbModel.Users.Where(x => x.Email == empleado.Personass.Correo).FirstOrDefault();
                                var userName = userDel.Id;
                                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbModel));
                                var rolAnterior = RoleManager.FindByName(empleado.Usuarioss.Rol).Id;

                                if (!RoleManager.RoleExists(empleados.Usuarioss.Rol))
                                {
                                    var role = new IdentityRole();
                                    role.Name = rol;
                                    RoleManager.Create(role);
                                }

                                var rolNuevo = RoleManager.FindByName(rol);

                                UserManager.RemoveFromRole(userName, rolAnterior);
                                UserManager.AddToRole(userDel.Id,rolNuevo.Name);

                                DbModel.Empleados.AddOrUpdate(empleados);
                                DbModel.Usuarios.AddOrUpdate(empleados.Usuarioss);
                                DbModel.Personas.AddOrUpdate(empleados.Personass);
                                DbModel.SaveChanges();

                                res = "Actualización realizada";
                                Session["res"] = res;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaEmpleado");
                            }
                            else
                            {
                                DbModel.Empleados.AddOrUpdate(empleados);
                                DbModel.Usuarios.AddOrUpdate(empleados.Usuarioss);
                                DbModel.Personas.AddOrUpdate(empleados.Personass);
                                DbModel.SaveChanges();

                                res = "Actualización realizada";
                                Session["res"] = res;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaEmpleado");
                            }
                        }
                        catch (Exception ex)
                        {
                            res = ex.Message;
                            ViewBag.res = res;
                            return RedirectToAction("ListaEmpleado");
                        }
                    }

                }
                else if (empleados.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var usuario = DbModel.Usuarios.ToList();
                    var persona = DbModel.Personas.ToList();
                    var empleado = DbModel.Empleados.Find(empleados.Id);

                    if (empleado != null)
                    {

                        try
                        {
                            var user = DbModel.Users.Where(x => x.Email == empleado.Personass.Correo).FirstOrDefault().Id;
                            var userDel = DbModel.Users.Where(x => x.Email == empleado.Personass.Correo).FirstOrDefault();
                            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbModel));
                            var rolDel = RoleManager.FindByName(empleado.Usuarioss.Rol).Id;
                            UserManager.RemoveFromRole(user, rolDel);

                            DbModel.Users.Remove(userDel);
                            DbModel.Usuarios.Remove(empleado.Usuarioss);
                            DbModel.Personas.Remove(empleado.Personass);
                            DbModel.Empleados.Remove(empleado);
                            DbModel.SaveChanges();

                            res = "Eliminación finalizada";
                            Session["res"] = res;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaEmpleado");
                        }
                        catch (Exception ex)
                        {
                            res = ex.Message;
                            ViewBag.res = res;
                            return RedirectToAction("ListaEmpleado");
                        }
                    }

                }
                else
                {
                    if (empleados != null)
                    {
                        // Aquí código para crear
                        try
                        {
                            empleados.FechaIngreso = DateTime.Now;
                            empleados.Usuarioss.Rol = rol;
                            empleados.Usuarioss.Password = password;

                            DbModel.Usuarios.Add(empleados.Usuarioss);
                            empleados.Personass.idUsuario = empleados.Usuarioss.idUsuario;
                            DbModel.Personas.Add(empleados.Personass);
                            DbModel.Empleados.Add(empleados);
                            
                            var user = new ApplicationUser { UserName = empleados.Usuarioss.User, Email = empleados.Personass.Correo, idPersona = empleados.Personass.idPersona, IdUsuario = empleados.Usuarioss.idUsuario};
                            var result = await UserManager.CreateAsync(user, empleados.Usuarioss.Password);
                            if (result.Succeeded)
                            {
                                DbModel.SaveChanges();
                                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                                await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", callbackUrl);
                                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbModel));

                                if (!RoleManager.RoleExists(empleados.Usuarioss.Rol))
                                {
                                    var role = new IdentityRole();
                                    role.Name = empleados.Usuarioss.Rol;
                                    RoleManager.Create(role);
                                }

                                rol = RoleManager.FindByName(empleados.Usuarioss.Rol).Name;
                                
                                var UsuarioNuevo = UserManager.FindByEmail(empleados.Personass.Correo).Id;
                                UserManager.AddToRole(UsuarioNuevo, rol);
                                
                                res = "Inserción realizada";
                                Session["res"] = res;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaEmpleado");
                            }
                            else
                            {
                                foreach (var error in result.Errors)
                                {
                                    Session["res"] = error;
                                }
                                return RedirectToAction("ListaEmpleado");
                            }
                        }
                        catch (Exception ex)
                        {
                            res = ex.Message;
                            Session["res"] = res;
                            return RedirectToAction("ListaEmpleado");
                        }
                    }
                    res = "Error";
                    ViewBag.res = res;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaEmpleado");


            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public ActionResult ListaEmpleado(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                //var personas = DbModel.Personas.ToList();
                //var usuarios = DbModel.Usuarios.ToList();
                //ViewBag.Personas = personas;
                //ViewBag.Usuarios = usuarios;
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Empleados.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Empleados = DbModel.Empleados.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();

                foreach (var item in _Empleados)
                {
                    item.Personass = (Persona)item.Personass;
                    item.Usuarioss = (Usuario)item.Usuarioss;
                }

                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorEmpleado = new PaginadorGenerico<Empleado>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Empleados
                };

                return View(_PaginadorEmpleado);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public ActionResult BuscarEmpleado(string param, int pagina = 1)
        {
            int _TotalRegistros = 0; 
            if (!string.IsNullOrWhiteSpace(param))
            {
                using (ApplicationDbContext DbModel = new ApplicationDbContext())
                {
                    var usuarios = DbModel.Usuarios.ToList();
                    var personas = DbModel.Personas.ToList();

                    _TotalRegistros = DbModel.Empleados.Where(x => x.Personass.Nombre.Contains(param) ||
                                                                    (x.Personass.Telefono.Contains(param)) ||
                                                                    (x.Personass.Estado.Contains(param)) ||
                                                                    (x.FechaIngreso.Month.ToString().Contains(param)) ||
                                                                    (x.FechaIngreso.Year.ToString().Contains(param)) ||
                                                                    (x.Usuarioss.Rol.Contains(param)) ||
                                                                    (x.Usuarioss.User.Contains(param)))
                                                                    .OrderBy(x => x.Id)
                                                                    .Count();

                    _Empleados = DbModel.Empleados.Where(x => x.Personass.Nombre.Contains(param) ||
                                                                    (x.Personass.Telefono.Contains(param)) ||
                                                                    (x.Personass.Estado.Contains(param)) ||
                                                                    (x.FechaIngreso.Month.ToString().Contains(param)) ||
                                                                    (x.FechaIngreso.Year.ToString().Contains(param)) ||
                                                                    (x.Usuarioss.Rol.Contains(param)) ||
                                                                    (x.Usuarioss.User.Contains(param)))
                                                                    .OrderBy(x => x.Id)
                                                                    .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                    .Take(_RegistrosPorPagina)
                                                                    .ToList();

                    var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);

                    _PaginadorEmpleado = new PaginadorGenerico<Empleado>()
                    {
                        RegistrosPorPagina = _RegistrosPorPagina,
                        TotalRegistros = _TotalRegistros,
                        TotalPaginas = _TotalPaginas,
                        PaginaActual = pagina,
                        Resultado = _Empleados
                    };
                    if (_TotalRegistros < 1)
                    {
                        ViewBag.error = "No hay resultados";
                        return RedirectToAction("ListaEmpleado");
                    }
                    else
                    {
                        return View("ListaEmpleado", _PaginadorEmpleado);
                    }
                }
            }
            else
            {
                ViewBag.error = "No hay resultados";
                return View("ListaEmpleado");
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
    }
}