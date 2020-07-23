using MerxProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{

    public class ProductoController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Inventario> _Productos;
        private PaginadorGenerico<Inventario> _PaginadorProductos;
        private List<Producto> _Producto;
        private PaginadorGenerico<Producto> _PaginadorProducto;

        // GET: Producto
        public ActionResult Tienda(int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Productos.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Productos = DbModel.Inventarios.Where(x=> x.Cantidad > 0)
                                                 .OrderBy(x => x.Producto.Nombre)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProductos = new PaginadorGenerico<Inventario>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Productos
                };
                // Enviamos a la Vista la 'Clase de paginación'
                return View(_PaginadorProductos);
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
                var producto = DbModel.Inventarios.Find(id);
                return View(producto);
            }
        }

        public ActionResult Buscar(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;

                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Inventarios.Where(x => x.Producto.Nombre.Contains(parameter) ||
                    (x.Producto.Descripcion.Contains(parameter)) ||
                    (x.Color.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMaterial.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMueble.Nombre.Contains(parameter)) ||
                    (x.Producto.Precio.ToString().Contains(parameter))).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Productos = DbModel.Inventarios.Where(x => x.Producto.Nombre.Contains(parameter) ||
                    (x.Producto.Descripcion.Contains(parameter)) ||
                    (x.Color.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMaterial.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMueble.Nombre.Contains(parameter)) ||
                    (x.Producto.Precio.ToString().Contains(parameter))).OrderBy(x => x.Producto.Nombre)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProductos = new PaginadorGenerico<Inventario>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Productos
                };

                if (_TotalRegistros < 1)
                {
                    ViewBag.alert = ("No hay resultados");
                    return View("Tienda");
                }
                else
                {
                    return View(_PaginadorProductos);
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

                            if(postedFile != null)
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
                            }
                            else
                            {
                                productos.Imagen = producto.Imagen;
                            }

                            try
                            {
                                
                                DbModel.Productos.AddOrUpdate(productos);
                                DbModel.SaveChanges();
                                resultado = "Actualización realizada";
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaProducto");

                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                Session["res"]= resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        Session["res"] = resultado;
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
                            DbModel.Productos.Remove(producto);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProducto");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        Session["res"] = resultado;
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
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaProducto");
                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                Session["res"] = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    resultado = "Error";
                    Session["res"] = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaProducto");


            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpProductosColor(int Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                var Colores = DbModel.Colores.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                ViewBag.Colores = Colores;

                ViewBag.title = "Inventario";
                var producto = DbModel.Productos.Find(Id);

                return View(producto);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpProductosColor(Producto productos, string color, int cantidad, int accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                Color colour = DbModel.Colores.Find(color);
                Inventario inv = new Inventario();
                inv.Color = colour;
                inv.Cantidad = cantidad;
                inv.Producto = productos;
                string resultado;
                try
                {
                    if (accion == 1)
                    {
                        DbModel.Inventarios.AddOrUpdate(inv);
                        DbModel.SaveChanges();
                        resultado = "Actualización realizada";
                        Session["res"] = resultado;
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaProducto");
                    }
                    if (accion == 2)
                    {
                        DbModel.Inventarios.Remove(inv);
                        DbModel.SaveChanges();
                        resultado = "Eliminación finalizada";
                        Session["res"] = resultado;
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaProducto");
                    }
                    if (accion == 3)
                    {
                        DbModel.Inventarios.Add(inv);
                        DbModel.SaveChanges();
                        resultado = "Inserción realizada";
                        Session["res"] = resultado;
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaProducto");
                    }
                }
                catch (Exception ex)
                {
                    resultado = ex.Message;
                    Session["res"] = resultado;
                    return RedirectToAction("ListaProducto");
                }
            }
            return RedirectToAction("ListaProducto");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaProducto(int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                var Colores = DbModel.Colores.ToList();


                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                ViewBag.Colores = Colores;



                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Productos.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Producto = DbModel.Productos.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProducto = new PaginadorGenerico<Producto>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Producto
                };

                return View(_PaginadorProducto);
            }
        }

        public ActionResult BuscarLista(string orden, string categoria, string parameter, int pagina = 1)
        {
           
            int _TotalRegistros = 0;
            var propertyInfo = typeof(Producto).GetProperty(parameter);
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;

                if (string.IsNullOrWhiteSpace(categoria) && string.IsNullOrWhiteSpace(orden) && string.IsNullOrWhiteSpace(parameter))
                {
                    _TotalRegistros = DbModel.Productos.Count();
                    // Obtenemos la 'página de registros' de la tabla Productos
                    _Producto = DbModel.Productos.OrderBy(x => x.Nombre)
                                                     .Skip((pagina - 1) * _RegistrosPorPagina)
                                                     .Take(_RegistrosPorPagina)
                                                     .ToList();
                    // Número total de páginas de la tabla Productos
                    var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                    // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                    _PaginadorProducto = new PaginadorGenerico<Producto>()
                    {
                        RegistrosPorPagina = _RegistrosPorPagina,
                        TotalRegistros = _TotalRegistros,
                        TotalPaginas = _TotalPaginas,
                        PaginaActual = pagina,
                        Resultado = _Producto
                    };
                    return View("ListaProducto", _PaginadorProductos);
                }
                else if(!string.IsNullOrWhiteSpace(categoria) && string.IsNullOrWhiteSpace(orden))
                {
                    orden = "Asc";
                }
                else if(string.IsNullOrWhiteSpace(categoria) && !string.IsNullOrWhiteSpace(orden))
                {
                    categoria = "Nombre";
                }

                switch (categoria)
                {
                    case "Precio":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x=> x.Precio).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.Precio).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.Precio)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.Precio)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProducto);

                    case "Nombre":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProductos);

                    case "CatMaterial":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMaterial.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMaterial.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.CategoriaMaterial.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.CategoriaMaterial.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProductos);

                    case "CatMueble":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMueble.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMueble.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.CategoriaMueble.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.CategoriaMueble.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProductos);
                        
                    default:
                        ViewBag.error = "No hay resultados";
                        _TotalRegistros = DbModel.Productos.Count();
                        // Obtenemos la 'página de registros' de la tabla Productos
                        _Producto = DbModel.Productos.OrderBy(x => x.Nombre)
                                                         .Skip((pagina - 1) * _RegistrosPorPagina)
                                                         .Take(_RegistrosPorPagina)
                                                         .ToList();
                        // Número total de páginas de la tabla Productos
                        var _TP = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                        // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                        _PaginadorProducto = new PaginadorGenerico<Producto>()
                        {
                            RegistrosPorPagina = _RegistrosPorPagina,
                            TotalRegistros = _TotalRegistros,
                            TotalPaginas = _TP,
                            PaginaActual = pagina,
                            Resultado = _Producto
                        };
                        return View("ListaProducto", _PaginadorProductos);
                }
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