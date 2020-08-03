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
    public class ComprasController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpCompras(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                ViewBag.Proveedores = DbModel.Proveedores.Include("Persona").ToList();
                ViewBag.Empleados = DbModel.Empleados.Include("Personass").Include("Usuarioss").ToList();
                if (accion == "1")
                {
                    var correo = User.Identity.Name;
                    var compra = new Compra();
                    compra.FechaRegistro = DateTime.Now;
                    compra.Estatus = 0;
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                    if (correo != "")
                    {
                        var Persona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
                        compra.Empleado = DbModel.Empleados.
                            Include("Personass").
                            FirstOrDefault(x => x.Personass.idPersona == Persona.idPersona);
                    }
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    return View("popUpCompras", compra);
                }
                else if (accion == "2")
                {
                    var compra = DbModel.Compras.Find(Id);
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    return View(compra);
                }
                else if (accion == "3")
                {
                    var compra = DbModel.Compras.Find(Id);
                    compra.Estatus = 2;
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    return View(compra);
                }
                else if (accion == "4")
                {
                    if (Id != null)
                    {
                        var detalleCompra = DbModel.DetalleCompra
                            .Include("Herramienta").Include("MateriaPrima").Include("Compra")
                            .Where(e => e.Compra.Id == Id).ToList();

                        return View("ListaDetalleCompra", detalleCompra);
                    }
                }
            }
            return RedirectToAction("popUpCompras");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpCompras(Compra compras, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (compras.Id > 0 && accion == "2")
                {
                    // Edición
                    var compra = DbModel.Compras.Find(compras.Id);

                    if (compra != null)
                    {
                        var proveedor = DbModel.Proveedores.Find(compras.Proveedor.Id);
                        var empleado = DbModel.Empleados.Find(compras.Empleado.Id);

                        if (proveedor != null && empleado != null)
                        {

                            compra.Proveedor = proveedor;
                            compra.Empleado = empleado;
                            try
                            {
                                DbModel.Compras.AddOrUpdate(compras);
                                DbModel.SaveChanges();
                                resultado = "Actualización realizada";
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaCompras");

                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaCompras");
                            }
                        }
                    }

                }
                else if (compras.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var compra = DbModel.Compras.Find(compras.Id);

                    if (compra != null)
                    {

                        try
                        {
                            DbModel.Compras.Remove(compra);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaCompras");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaCompras");
                        }
                    }
                }
                else
                {
                    if (compras != null)
                    {
                        var proveedor = DbModel.Proveedores.Find(compras.Proveedor.Id);
                        var empleado = DbModel.Empleados.Find(compras.Empleado.Id);

                        if (proveedor != null && empleado != null)
                        {

                            compras.Proveedor = proveedor;
                            compras.Empleado = empleado;
                            // Aquí código para crear
                            try
                            {

                                DbModel.Compras.Add(compras);
                                DbModel.SaveChanges();
                                resultado = "Inserción realizada";
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaCompras");
                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaCompras");
                            }
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaCompras");
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaCompras()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var compras = DbModel.Compras.Include("Empleado.Personass").Include("Proveedor.Persona").ToList();
                foreach(Compra compra in compras)
                {
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                }

                return View(compras);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaDetalleCompra(int? idCompra)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (idCompra != null)
                {
                    var detalleCompra = DbModel.DetalleCompra
                        .Include("Herramienta").Include("MateriaPrima").Include("Compra")
                        .Where(e => e.Compra.Id == idCompra).ToList();

                    return View("ListaDetalleCompra", detalleCompra);
                }
                return View("ListaDetalleCompra");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpDetalleCompra(int? Id, string accion, string tipo)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                ViewBag.tipo = tipo;
                if (accion == "1")
                {
                    if (tipo != null)
                    {
                        if (tipo.Equals("H"))
                        {
                            ViewBag.Articulos = DbModel.Herramientas.ToList();
                        }
                        else if (tipo.Equals("MP"))
                        {
                            ViewBag.Articulos = DbModel.Materiales.ToList();
                        }
                    }
                    var detalleCompra = new DetalleCompra();
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    //return PartialView("_popUpDetalleCompra", detalleCompra);
                    return View("popUpDetalleCompra", detalleCompra);
                }
                else if (accion == "2")
                {
                    if (tipo != null)
                    {
                        if (tipo.Equals("H"))
                        {
                            ViewBag.Articulos = DbModel.Herramientas.ToList();
                        }
                        else if (tipo.Equals("MP"))
                        {
                            ViewBag.Articulos = DbModel.Materiales.ToList();
                        }
                    }
                    var detalleCompra = DbModel.DetalleCompra.Find(Id);

                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    //return PartialView("_popUpDetalleCompra", detalleCompra);
                    return View("popUpDetalleCompra", detalleCompra);
                }
                else if (accion == "3")
                {
                    var detalleCompra = DbModel.DetalleCompra.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    return PartialView(detalleCompra);
                }
            }
            return RedirectToAction("popUpCompras");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpDetalleCompra(DetalleCompra detalleCompra, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (detalleCompra.Id > 0 && accion == "2")
                {
                    // Edición
                    var detCompra = DbModel.DetalleCompra.Find(detalleCompra.Id);

                    if (detCompra != null)
                    {
                        try
                        {
                            DbModel.DetalleCompra.AddOrUpdate(detalleCompra);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("popUpCompras");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("popUpCompras");
                        }
                    }

                }
                else if (detalleCompra.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var detCompra = DbModel.DetalleCompra.Find(detalleCompra.Id);

                    if (detCompra != null)
                    {

                        try
                        {
                            DbModel.DetalleCompra.Remove(detCompra);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaCompras");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaCompras");
                        }
                    }
                }
                else
                {
                    if (detalleCompra != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.DetalleCompra.Add(detalleCompra);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("PopUpCompras");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("PopUpCompras");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaCompras");
            }
        }

        public ActionResult ListaArticulos(int tipo)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if(tipo == 0)
                {
                    var Articulos = DbModel.Herramientas.ToList();
                    return Json(Articulos, JsonRequestBehavior.AllowGet);
                }
                else if (tipo == 1)
                {
                    var Articulos = DbModel.Materiales.ToList();
                    return Json(Articulos, JsonRequestBehavior.AllowGet);
                }
                return PartialView("_ListaDetalleCompra");
            }
        }
    }
}