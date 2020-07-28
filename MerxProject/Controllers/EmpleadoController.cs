using MerxProject.Models;
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
        // GET: Empleado
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
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
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    return View(empleado);
                }
                else if (accion == "3")
                {
                    var empleado = DbModel.Empleados.Find(Id);
                    empleado.Usuarioss = (Usuario)empleado.Usuarioss;
                    empleado.Personass = (Persona)empleado.Personass;
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    return View(empleado);
                }
            }
            return RedirectToAction("ListaEmpleado");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> PopUpEmpleados(Empleado empleados, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
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
                            DbModel.Empleados.AddOrUpdate(empleados);
                            DbModel.Usuarios.AddOrUpdate(empleados.Usuarioss);
                            DbModel.Personas.AddOrUpdate(empleados.Personass);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaEmpleado");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaEmpleado");
                        }
                    }

                }
                else if (empleados.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var empleado = DbModel.Empleados.Find(empleados.Id);

                    if (empleado != null)
                    {

                        try
                        {
                            DbModel.Empleados.Remove(empleado);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaEmpleado");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaEmpleado");
                        }
                    }

                }
                else
                {
                    //string dir = Server.MapPath("~/Content/assets/img/Materiales");
                    //if (!Directory.Exists(dir))
                    //{
                    //    Directory.CreateDirectory(dir);
                    //}

                    ////var originalFile = Path.GetFileName(postedFile.FileName);
                    ////string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    ////var path = Path.Combine(dir, fileId);
                    ////postedFile.SaveAs(path + postedFile.FileName);

               
                    if (empleados != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.Empleados.Add(empleados);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaEmpleado");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaEmpleado");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaEmpleado");


            }
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaEmpleado()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                //var personas = DbModel.Personas.ToList();
                //var usuarios = DbModel.Usuarios.ToList();
                //ViewBag.Personas = personas;
                //ViewBag.Usuarios = usuarios;
                var emp = DbModel.Empleados.ToList();
                foreach (var item in emp)
                {
                    item.Personass = (Persona)item.Personass;
                    item.Usuarioss = (Usuario)item.Usuarioss;
                }
                return View(emp);
            }
        }
    }
}