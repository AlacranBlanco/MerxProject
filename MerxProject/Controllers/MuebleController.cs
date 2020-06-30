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
    public class MuebleController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpMuebles(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (accion == "1")
                {
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View();
                }
                else if (accion == "2")
                {
                    var material = DbModel.Muebles.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(material);
                }
                else if (accion == "3")
                {
                    var material = DbModel.Muebles.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(material);
                }
            }
            return RedirectToAction("ListaMueble");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpMuebles(Mueble muebles, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (muebles.Id > 0 && accion == "2")
                {
                    // Edición
                    var mueble = DbModel.Muebles.Find(muebles.Id);

                    if (mueble != null)
                    {
                        try
                        {
                            DbModel.Muebles.AddOrUpdate(mueble);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMueble");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }

                }
                else if (muebles.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var mueble = DbModel.Muebles.Find(muebles.Id);

                    if (mueble != null)
                    {

                        try
                        {
                            DbModel.Muebles.Remove(mueble);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Muebles");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var originalFile = Path.GetFileName(postedFile.FileName);
                    string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    var path = Path.Combine(dir, fileId);
                    postedFile.SaveAs(path + postedFile.FileName);
                    muebles.Imagen = fileId + postedFile.FileName;


                    if (muebles != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.Muebles.Add(muebles);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaMaterial");


            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaMueble()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var muebles = DbModel.Muebles.ToList();
                return View(muebles);
            }
        }
    }
}