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
                if (accion == "1")
                {
                    ViewBag.Proveedores = DbModel.Proveedores.Include("Persona").ToList();
                    ViewBag.Empleados = DbModel.Empleados.Include("Personas").ToList();
                    var compra = new Compra();
                    compra.FechaRegistro = DateTime.Now;
                    compra.Estatus = 0;
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    return View(compra);
                }
                else if (accion == "2")
                {
                    var compra = DbModel.Compras.Find(Id);
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                    ViewBag.Proveedores = DbModel.Proveedores.Include("Persona").ToList();
                    ViewBag.Empleados = DbModel.Empleados.Include("Personas").ToList();
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
            }
            return RedirectToAction("ListaCompras");
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
                var compras = DbModel.Compras.Include("Empleado.Personas").Include("Proveedor.Persona").ToList();
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
                ViewBag.Herramientas = DbModel.Herramientas.ToList();
                ViewBag.MateriasPrimas = DbModel.Materiales.ToList();

                if (idCompra != null)
                {
                    var detalleCompra = DbModel.DetalleCompra
                        .Include("Herramienta").Include("MateriaPrima").Include("Compra")
                        .Where(e => e.IdCompra == idCompra).ToList();
                    return PartialView("_ListaDetalleCompra", detalleCompra);
                }
                return PartialView("_ListaDetalleCompra");
            }
        }
    }
}