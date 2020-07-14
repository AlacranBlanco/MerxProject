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
    public class ProductoController : Controller
    {
        // GET: Producto
        public ActionResult Tienda()
        {
            return View();
        }

        public ActionResult MostrarTodos()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                //Obtiene todos los productos y los manda a listado
                var productos = DbModel.Productos.ToList();
                return PartialView("_MostrarTodos", productos);
            }
        }

        public ActionResult VistaProducto(int? id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                //Obtiene el producto que coincida con el ID seleccionado de la lista MostrarTodos(). 
                var producto = DbModel.Productos.Find(id);
                return View(producto);
            }
        }

        public ActionResult Buscar(string parameter)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                //Searh es un método de NinjaNye.SearchExtensions: Colección de métodos a IQueryable y IEnumerable que facilitan las búsquedas. 

                //SearchExtension
                //var productos1 = DbModel.Productos.Search(x => x.Nombre,
                //    x => x.Descripcion, x => x.Color, x => "" + x.Precio).
                //    Containing(parameter).ToList();

                var productos2 = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                    (x.Descripcion.Contains(parameter)) ||
                    (x.Color.Contains(parameter)) ||
                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                    (x.Precio.ToString().Contains(parameter))).ToList();

                if (productos2.Count() < 1)
                {
                    ViewBag.alert = ("No hay resultados");
                    return View("Store");
                }
                else
                {

                    return View(productos2);
                }
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpProductos(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                if (accion == "1")
                {
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View();
                }
                else if (accion == "2")
                {
                    var producto = DbModel.Productos.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(producto);
                }
                else if (accion == "3")
                {
                    var producto = DbModel.Productos.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(producto);
                }
            }
            return RedirectToAction("ListaMueble");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpProductos(Producto productos, int idMueble, int idMaterial, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (productos.Id > 0 && accion == "2")
                {
                    // Edición
                    var producto = DbModel.Productos.Find(productos.Id);
                    if (producto != null)   
                    {

                        var material = DbModel.Materiales.Find(idMaterial);
                        var mueble = DbModel.Muebles.Find(idMueble);

                        if (material != null && mueble != null)
                        {

                            productos.CategoriaMaterial = material;
                            productos.CategoriaMueble = mueble;

                            try
                            {
                                
                                DbModel.Productos.AddOrUpdate(productos);
                                DbModel.SaveChanges();
                                resultado = "Actualización realizada";
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");

                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        ViewBag.res = resultado;
                        return RedirectToAction("ListaProducto");
                    }

                }
                else if (productos.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var producto = DbModel.Productos.Find(productos.Id);

                    if (producto != null)
                    {

                        try
                        {
                            DbModel.Productos.Remove(productos);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        ViewBag.res = resultado;
                        return RedirectToAction("ListaProducto");
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Productos");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var originalFile = Path.GetFileName(postedFile.FileName);
                    string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    var path = Path.Combine(dir, fileId);
                    postedFile.SaveAs(path + postedFile.FileName);
                    productos.Imagen = fileId + postedFile.FileName;


                    if (productos != null)
                    {
                        var material = DbModel.Materiales.Find(idMaterial);
                        var mueble = DbModel.Muebles.Find(idMueble);

                        if (material != null && mueble != null)
                        {

                            productos.CategoriaMaterial = material;
                            productos.CategoriaMueble = mueble;
                            // Aquí código para crear
                            try
                            {

                                DbModel.Productos.Add(productos);
                                DbModel.SaveChanges();
                                resultado = "Inserción realizada";
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaProducto");


            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaProducto()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;

                var productos = DbModel.Productos.ToList();
                return View(productos);
            }
        }

        #region Shelve Producto

        [AllowAnonymous]
        [HttpGet]
        public ActionResult  ShelveProduct1()
        {
            return View("");
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult ShelveProduct2()
        {
            return View("");
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult ShelveProduct3()
        {
            return View("");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ShelveProduct4()
        {
            return View("");
        }

        #endregion

    }
}