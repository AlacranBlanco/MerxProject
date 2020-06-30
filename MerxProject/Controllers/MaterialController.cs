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
    public class MaterialController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpMateriales(int? Id, string accion)
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
                    var material = DbModel.Materiales.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(material);
                }
                else if (accion == "3")
                {
                    var material = DbModel.Materiales.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(material);
                }
            }
            return RedirectToAction("ListaMaterial");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpMateriales(Material materiales, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (materiales.Id > 0 && accion == "2")
                {
                    // Edición
                    var material = DbModel.Materiales.Find(materiales.Id);

                    if (material != null)
                    {
                        try
                        {
                            DbModel.Materiales.AddOrUpdate(materiales);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMaterial");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMaterial");
                        }
                    }

                }
                else if (materiales.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var material = DbModel.Materiales.Find(materiales.Id);

                    if (material != null)
                    {

                        try
                        {
                            DbModel.Materiales.Remove(material);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMaterial");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMaterial");
                        }
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Materiales");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var originalFile = Path.GetFileName(postedFile.FileName);
                    string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    var path = Path.Combine(dir, fileId);
                    postedFile.SaveAs(path + postedFile.FileName);
                    materiales.Imagen = fileId + postedFile.FileName;


                    if (materiales != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.Materiales.Add(materiales);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMaterial");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMaterial");
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
        public ActionResult ListaMaterial()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var materiales = DbModel.Materiales.ToList();
                return View(materiales);
            }
        }
    }
}
