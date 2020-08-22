using MerxProject.Models;
using MerxProject.Models.ProductosFavorito;
using MerxProject.Models.CarritoCompras;
using MerxProject.Models.TiendaViewModels;
using MerxProject.Models.VentaViewModels;
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
        //public ActionResult Tienda(int pagina = 1)

        ApplicationDbContext DbModel;


        public ProductoController()
        {
            this.DbModel = new ApplicationDbContext();
        }

        // GET: Producto
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Tienda(int? NoLogIn, int? OutStock, string colorNombre = null, int pag = 1)
        {
            if (pag <= 0) pag = 1;

            if (OutStock != null)
            {
                ViewBag.outStock = 1;
                ViewBag.nombreColor = colorNombre;
            }
            else
            {
                ViewBag.outStock = 0;
            }

            var email = User.Identity.Name;

            if (email != "" && email.Contains("@"))
            {
                ViewBag.NoLogIn = 1;
                var usuarioLoggeado = DbModel.Personas.FirstOrDefault(x => x.Correo == email);


                TiendaViewModel tiendaViewModel = new TiendaViewModel();
                tiendaViewModel.ProductoCollection = DbModel.Productos.ToList();

                tiendaViewModel.ProductosFavoritosColelction = DbModel.ProductosFavoritos.Where(x => x.idPersona == usuarioLoggeado.idPersona).OrderBy(x => x.idProducto).ToList();
                tiendaViewModel.ProductosCarritoCollection = DbModel.CarritoCompras.Where(x => x.idPersona == usuarioLoggeado.idPersona).OrderBy(x => x.idProducto).ToList();

                tiendaViewModel.ProductosFavId = new List<int>();
                tiendaViewModel.ProductosCarrito = new List<int>();

                int idxAux = 0;
                foreach (var item in tiendaViewModel.ProductoCollection)
                {
                    if (idxAux < tiendaViewModel.ProductosFavoritosColelction.Count)
                    {
                        if (item.Id == tiendaViewModel.ProductosFavoritosColelction[idxAux].idProducto)
                        {
                            tiendaViewModel.ProductosFavId.Add(item.Id);

                            idxAux++;
                        }
                        else
                        {

                            tiendaViewModel.ProductosFavId.Add(-1);
                        }
                    }
                    else
                    {
                        tiendaViewModel.ProductosFavId.Add(-1);
                    }

                }
                int idxAux2 = 0;

                foreach (var item in tiendaViewModel.ProductoCollection)
                {
                    if (idxAux2 < tiendaViewModel.ProductosCarritoCollection.Count)
                    {
                        if (item.Id == tiendaViewModel.ProductosCarritoCollection[idxAux2].idProducto)
                        {
                            tiendaViewModel.ProductosCarrito.Add(item.Id);

                            idxAux2++;
                        }
                        else
                        {

                            tiendaViewModel.ProductosCarrito.Add(-1);
                        }
                    }
                    else
                    {
                        tiendaViewModel.ProductosCarrito.Add(-1);
                    }

                }
                ViewBag.pagina = pag;
                return View(tiendaViewModel);
            }
            else
            {
                ViewBag.NoLogIn = 0;
                TiendaViewModel tiendaViewModel = new TiendaViewModel();
                tiendaViewModel.ProductoCollection = DbModel.Productos.ToList();
                ViewBag.pagina = pag;
                return View(tiendaViewModel);
            }
        }

        [HttpPost]
        public ActionResult Tienda()
        {
            return View();
        }


        public ActionResult MostrarTodos(int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Productos.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Productos = DbModel.Inventarios.Where(x => x.Cantidad > 0)
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

        [HttpGet]
        public ActionResult popUpProductos(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.Where(x => x.Piezas == true).ToList();
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

                            if (postedFile != null)
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
                            var inventario = DbModel.Inventarios.Where(x => x.Producto.Id == producto.Id && x.Cantidad > 0).Count();
                            if (inventario < 1)
                            {
                                var procesosActuales = DbModel.Procesos.Where(x => x.Inventario.Producto.Id == producto.Id).Count();
                                if (procesosActuales < 1)
                                {
                                    var inventarios = DbModel.Inventarios.Where(x => x.Producto.Id == producto.Id).ToList();
                                    DbModel.Inventarios.RemoveRange(inventarios);
                                    DbModel.Productos.Remove(producto);
                                    DbModel.SaveChanges();
                                    resultado = "Eliminación finalizada";
                                    Session["res"] = resultado;
                                    Session["tipo"] = "Exito";
                                    return RedirectToAction("ListaProducto");
                                }
                                else
                                {
                                    resultado = "Un producto de este tipo está en proceso. No se puede eliminar";
                                    Session["res"] = resultado;
                                    return RedirectToAction("ListaProducto");
                                }
                            }
                            else
                            {
                                resultado = "Aún quedan productos en inventario, no se puede eliminar";
                                Session["res"] = resultado;
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
                    else
                    {
                        resultado = "Error";
                        Session["res"] = resultado;
                        return RedirectToAction("ListaProducto");
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Productos/");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    var path = Path.Combine(dir, postedFile.FileName);
                    postedFile.SaveAs(path + postedFile.FileName);
                    productos.Imagen = "~/Content/assets/img/Productos/" + postedFile.FileName;
                    

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
                                var colores = DbModel.Colores.ToList();
                                var inv = new Inventario()
                                {
                                    Cantidad = 0,
                                    Color = colores.Where(x => x.Nombre == "Natural").FirstOrDefault(),
                                    Producto = productos
                                };
                                DbModel.Inventarios.Add(inv);
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

        [HttpGet]
        public ActionResult popUpProductosColor(int Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                Inventario inv = new Inventario();

                var Colores = DbModel.Colores.ToList();
                var Material = DbModel.Materiales.ToList();

                var ColoresHechos = new List<Inventario>();
                var producto = DbModel.Productos.Find(Id);
                var inventarios = DbModel.Inventarios.Where(x => x.Producto.CategoriaMaterial.Id == producto.CategoriaMaterial.Id).ToList();
                var invId = new List<Inventario>();

                foreach (var item in inventarios)
                {
                    if (item.Producto != null)
                    {
                        if (producto.Id == item.Producto.Id)
                        {
                            invId.Add(item);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                foreach (var item in invId)
                {
                    foreach (var color in Colores)
                    {
                        if (item.Color.Id == color.Id)
                        {
                            ColoresHechos.Add(item);
                        }
                    }
                }

                foreach (var item in ColoresHechos)
                {
                    Colores.Remove(item.Color);
                }

                int suma = 0;
                foreach (var item in invId)
                {
                    suma += item.Cantidad;
                }


                ViewBag.Colores = Colores;
                ViewBag.productoId = producto.Id;
                ViewBag.invId = invId;
                ViewBag.total = suma;

                ViewBag.title = "Inventario";
                ViewBag.subtitle = producto.Nombre;


                return View(inv);
            }
        }

        [HttpPost]
        public async Task<ActionResult> popUpProductosColor(int productoId, string cantidad, string newColor, string radio, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                Inventario inv = new Inventario();
                string resultado;
                try
                {
                    if (accion == "Editar")
                    {
                        if (radio != null)
                        {
                            inv = DbModel.Inventarios.Find(Convert.ToInt16(radio));
                            inv.Cantidad = Convert.ToInt16(cantidad);

                            DbModel.Inventarios.AddOrUpdate(inv);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProducto");
                        }
                        else if (newColor != null && radio == null)
                        {
                            var productos = DbModel.Productos.Find(productoId);
                            inv.Producto = productos;

                            Color colour = new Color();
                            colour = DbModel.Colores.Find(Convert.ToInt16(newColor));
                            inv.Color = colour;

                            inv.Cantidad = Convert.ToInt16(cantidad);

                            DbModel.Inventarios.Add(inv);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        var Colores = DbModel.Colores.ToList();
                        inv = DbModel.Inventarios.Find(Convert.ToInt16(radio));
                        if (DbModel.Procesos.Where(x => x.Inventario.Id == inv.Id).Count() < 1 && inv.Color.Nombre != "Natural")
                        {
                            DbModel.Inventarios.Remove(inv);
                            DbModel.SaveChanges();
                            resultado = "Eliminación realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProducto");
                        }
                        else
                        {
                            Session["res"] = "Hay productos en proceso con este color o es un color predeterminado";
                            return RedirectToAction("ListaProducto");
                        }

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
        [Authorize(Roles = "Administrador, Empleado")]
        public ActionResult ListaProducto(int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
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
                return View("ListaProducto", _PaginadorProducto);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ShelveProduct1()
        {
            return View("");
        }

        [Authorize(Roles = "Administrador, Empleado")]
        public ActionResult BuscarLista(string orden, string categoria, string parameter, int pagina = 1)
        {

            int _TotalRegistros = 0;
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
                    return View("ListaProducto", _PaginadorProducto);
                }
                else if (string.IsNullOrWhiteSpace(categoria) && string.IsNullOrWhiteSpace(orden) && !string.IsNullOrWhiteSpace(parameter))
                {
                    _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                     (x.Precio.ToString().Contains(parameter)) ||
                                                     (x.Descripcion.Contains(parameter)) ||
                                                     (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                     (x.CategoriaMueble.Nombre.Contains(parameter)))
                                                     .Count();
                    if (_TotalRegistros > 0)
                    {


                        // Obtenemos la 'página de registros' de la tabla Productos
                        _Producto = DbModel.Productos.OrderBy(x => x.Nombre)
                                                         .Where(x => x.Nombre.Contains(parameter) ||
                                                         (x.Precio.ToString().Contains(parameter)) ||
                                                         (x.Descripcion.Contains(parameter)) ||
                                                         (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                         (x.CategoriaMueble.Nombre.Contains(parameter)))
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
                        return View("ListaProducto", _PaginadorProducto);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(categoria) && string.IsNullOrWhiteSpace(orden))
                {
                    orden = "Asc";
                }
                else if (string.IsNullOrWhiteSpace(categoria) && !string.IsNullOrWhiteSpace(orden))
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
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.Precio).Count();
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
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.Precio)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
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
                                Session["res"] = "No hay resultados";
                                return RedirectToAction("ListaProducto");
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
                                Session["res"] = "No hay resultados";
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProducto);

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
                                Session["res"] = "No hay resultados";
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProducto);

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
                                Session["res"] = "No hay resultados";
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProducto);

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
                        return View("ListaProducto", _PaginadorProducto);
                }
            }
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public ActionResult Bamboo1(int? OutStock, int? Id, int? idProducto, string name, string colorNombre = null)
        {


            if (Id != null)
            { 
                if (Id.Value == 0)
                {
                    name = DbModel.Productos.FirstOrDefault(x => x.Id == idProducto).Nombre;
                }
                else
                {
                    var material = DbModel.Materiales.FirstOrDefault(x => x.Id == Id.Value);
                    ViewBag.precioMaterial = material.Precio;
                    ViewBag.imgRoute = material.Imagen;
                    ViewBag.idMaterial = Id.Value;
                    ViewBag.prodcutoId = DbModel.Productos.FirstOrDefault(x => x.Id == idProducto).Id;
                    name = DbModel.Productos.FirstOrDefault(x => x.Id == idProducto).Nombre;
                }
               
            }

            if (OutStock != null)
            {
                ViewBag.outStock = 1;
                ViewBag.nombreColor = colorNombre;
            }
            else
            {
                ViewBag.outStock = 0;
            }

            var email = User.Identity.Name;

            if (name != null)
            {

                if (email != "")
                {
                    var usuarioLoggeado = DbModel.Personas.FirstOrDefault(x => x.Correo == email);
                    var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == usuarioLoggeado.idPersona).ToList();
                    ViewBag.cantidadCarrito = cantidadCarrito.Count;
                    ViewBag.NoLogIn = 1;

                    var Colores = DbModel.Colores.ToList();
                    var Materiales = DbModel.Materiales.ToList();
                    VentaViewModels ventaViewModels = new VentaViewModels();
                    DbModel.Muebles.ToList();
                    ventaViewModels.Producto = DbModel.Productos.FirstOrDefault(x => x.Nombre == name);
                    ViewBag.prodcutoId = ventaViewModels.Producto.Id;
                    ventaViewModels.InventarioColorCollection = DbModel.Inventarios.Where(x => x.Producto.Id == ventaViewModels.Producto.Id && x.Material == null).ToList();
                    ventaViewModels.InventarioMaterialCollection = DbModel.Inventarios.Where(x => x.Producto.Id == ventaViewModels.Producto.Id && x.Color == null).ToList();
                    ventaViewModels.ColorsCollection = new List<Color>();
                    ventaViewModels.MaterialCollection = new List<Material>();
                    //foreach (var item in ventaViewModels.InventarioCollection)
                    //{
                    //    if (item.Color != null)
                    //    {
                    //        var colorDetail = DbModel.Colores.FirstOrDefault(x => x.Id == item.Color.Id);
                    //        ventaViewModels.ColorsCollection.Add(colorDetail);
                    //    }
                    //    else
                    //    {
                    //        var MaterialDetail = DbModel.Materiales.FirstOrDefault(x => x.Id == item.Material.Id);
                    //        ventaViewModels.MaterialCollection.Add(MaterialDetail);
                    //    }
                    //}

                    Material mat = new Material();
                    mat.Id = 0;
                    mat.Nombre = "Sin Material";

                    ventaViewModels.MaterialCollection.Add(mat);

                    foreach (var item in ventaViewModels.InventarioMaterialCollection)
                    {
                            var MaterialDetail = DbModel.Materiales.FirstOrDefault(x => x.Id == item.Material.Id);
                            ventaViewModels.MaterialCollection.Add(MaterialDetail);
                        
                    }
                    ViewBag.materialList = new SelectList(ventaViewModels.MaterialCollection, "Id", "Nombre");
                    //ViewBag.materialList = ventaViewModels.MaterialCollection;
                    ventaViewModels.ProductosFavoritosColelction = new List<ProductosFavoritos>();

                    ventaViewModels.ProductosFavoritosColelction = DbModel.ProductosFavoritos.Where(x => x.idPersona == usuarioLoggeado.idPersona).OrderBy(x => x.idProducto).ToList();

                    ventaViewModels.ProductosFavId = new List<int>();

                    foreach (var item in ventaViewModels.ProductosFavoritosColelction)
                    {

                        if (item.idProducto == ventaViewModels.Producto.Id)
                        {
                            ventaViewModels.ProductosFavId.Add(item.idProducto);
                            break;
                        }



                    }

                    return View(ventaViewModels);
                }
                else
                {
                    ViewBag.NoLogIn = 0;
                    VentaViewModels ventaViewModels = new VentaViewModels();
                    ventaViewModels.Producto = DbModel.Productos.FirstOrDefault(x => x.Id == idProducto.Value);
                    ViewBag.prodcutoId = ventaViewModels.Producto.Id;
                    ventaViewModels.InventarioColorCollection = DbModel.Inventarios.Where(x => x.Producto.Id == ventaViewModels.Producto.Id && x.Material == null).ToList();
                    ventaViewModels.InventarioMaterialCollection = DbModel.Inventarios.Where(x => x.Producto.Id == ventaViewModels.Producto.Id && x.Color == null).ToList();
                    ventaViewModels.ColorsCollection = new List<Color>();
                    ViewBag.materialList = ventaViewModels.InventarioMaterialCollection;
                    //foreach (var item in ventaViewModels.InventarioColorCollection)
                    //{
                    //    var colorDetail = DbModel.Colores.FirstOrDefault(x => x.Id == item.Color.Id);
                    //    ventaViewModels.ColorsCollection.Add(colorDetail);
                    //}

                    return View(ventaViewModels);
                }


            }
            else
            {

                return Redirect("Tienda");
            }



        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult AgregarProductoFavorito(int? pagi, Producto producto, int accion)
        {
            var correo = User.Identity.Name;
            if (correo != "")
            {
                if (accion == 1)
                {
                    ProductosFavoritos productosFavoritos = new ProductosFavoritos();
                    using (this.DbModel = new ApplicationDbContext())
                    {

                        var idPersonas = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
                        var nombreProdcuto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                        productosFavoritos.Nombre = nombreProdcuto.Nombre;
                        productosFavoritos.idProducto = producto.Id;
                        productosFavoritos.idPersona = idPersonas.idPersona;
                        DbModel.ProductosFavoritos.Add(productosFavoritos);
                        DbModel.SaveChanges();
                    }
                    if (pagi != null)
                    {
                        return RedirectToAction("Tienda", new { pag = pagi });
                    }
                    else
                    {

                        using (this.DbModel = new ApplicationDbContext())
                        {

                            var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                            return RedirectToAction("Bamboo1", new { name = nombreProducto.Nombre });
                        }
                    }

                }
                else if (accion == 2)
                {
                    // Eliminación
                    var productoFav = DbModel.ProductosFavoritos.FirstOrDefault(x => x.idProducto == producto.Id);

                    if (productoFav != null)
                    {
                        DbModel.ProductosFavoritos.Remove(productoFav);
                        DbModel.SaveChanges();

                        if (pagi != null)
                        {
                            return RedirectToAction("Tienda", new { pag = pagi });
                        }
                        else
                        {
                            var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                            return RedirectToAction("Bamboo1", new { name = nombreProducto.Nombre });
                        }

                    }
                }

            }
            else
            {
                // El usuario no ha sido logeado, le mandamos un mensaje
                ViewBag.NoLogIn = 0;
                ViewBag.pagina = 1;
                Tienda(0, 1);
                return View("Tienda");
            }



            return View("Tienda");

        }


        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> AgregarProductoCarrito(int? pagi, int? cantidadProducto, int? idMaterial, string esCarrito, string color,  Producto producto, int accion, string SvgColor)
        {
            var correo = User.Identity.Name;
            var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
            Color Color = new Color();
            Material Material = new Material();

            if (correo != "")
            {
                if (idMaterial != null && idMaterial > 0)
                {
                    Material = DbModel.Materiales.FirstOrDefault(x => x.Id == idMaterial.Value);
                }

                if (accion == 1)
                {

                    if (cantidadProducto != null)
                    {

                        

                        if (string.IsNullOrEmpty(color))
                        {
                            Color = DbModel.Colores.FirstOrDefault(x => x.Nombre == "Natural");
                        }
                        else
                        {
                            Color = DbModel.Colores.FirstOrDefault(x => x.Codigo == color);
                        }

                        var inventarioColor = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Color.Id == Color.Id);
                        var inventarioMaterial = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Material.Id == Material.Id);
                        var carritoCantidad = new CarritoCompra();
                        if (Material.Id > 0)
                        {
                         carritoCantidad = DbModel.CarritoCompras.FirstOrDefault(x => x.idProducto == producto.Id && x.ColorNombre == Color.Nombre && x.MaterialNombre == Material.Nombre && x.idPersona == persona.idPersona);
                        }
                        else
                        {
                         carritoCantidad = DbModel.CarritoCompras.FirstOrDefault(x => x.idProducto == producto.Id && x.ColorNombre == Color.Nombre && x.MaterialNombre == null && x.idPersona == persona.idPersona);

                        }
                        var carritoCantidadMaterial = DbModel.CarritoCompras.FirstOrDefault(x => x.idProducto == producto.Id && x.ColorNombre == Color.Nombre && x.idPersona == persona.idPersona);

                        if (esCarrito == "True")
                        {

                           

                            if (Material.Id > 0)
                            {
                                //bool aux = false, aux1 = false;
                                //var productoStock = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                //var stockSuficiente = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona && x.idProducto == producto.Id).ToList();
                                //var cantidadSuficiente = 0;
                                //foreach (var item in stockSuficiente)
                                //{
                                //    cantidadSuficiente += item.Cantidad;
                                //}

                                //if (cantidadSuficiente + 1 > productoStock.Cantidad)
                                //{
                                //    if (carritoCantidad.Cantidad + 1 > inventarioColor.Cantidad)
                                //    {
                                //        aux = true;
                                //    }

                                //    if (carritoCantidad.Cantidad + 1 > inventarioMaterial.Cantidad)
                                //    {

                                //        aux1 = true;
                                //    }

                                //    if (aux || aux1)
                                //    {
                                //        return RedirectToAction("IndexTienda", "Tienda", new { outStock = 1 });
                                //    }
                                //}
                            }
                            //else
                            //{
                            //    if (carritoCantidad.Cantidad + 1 > inventarioColor.Cantidad)
                            //    {
                            //        return RedirectToAction("IndexTienda", "Tienda", new { outStock = 1 });
                            //    }
                            //}

                        }
                        else
                        {
                            if (Material.Id > 0)
                            {
                                //string NoExistsBth = "";
                                //var productoStock = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                //var stockSuficiente = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona && x.idProducto == producto.Id).ToList();
                                //var cantidadSuficiente = 0;
                                //foreach (var item in stockSuficiente)
                                //{
                                //    cantidadSuficiente += item.Cantidad;
                                //}

                                //if (cantidadSuficiente + 1 <= productoStock.Cantidad)
                                //{
                                //    if (carritoCantidad != null)
                                //    {
                                //        bool aux = false, aux1 = false;
                                //    if (carritoCantidad.Cantidad + 1 > inventarioColor.Cantidad)
                                //    {

                                //        NoExistsBth = carritoCantidad.ColorNombre;
                                //        aux = true;
                                //    }

                                //    if (carritoCantidad.Cantidad + 1 > inventarioMaterial.Cantidad)
                                //    {
                                //        NoExistsBth = carritoCantidad.MaterialNombre;
                                //        aux1 = true;
                                //    }

                                //    if (aux1 && aux)
                                //    {
                                //        NoExistsBth = "";
                                //        NoExistsBth = carritoCantidad.ColorNombre + " y " + carritoCantidad.MaterialNombre;
                                //    }


                                //        if (pagi != null)
                                //        {
                                //            return RedirectToAction("Tienda", new { Outstock = 1, pag = pagi, colorNombre = NoExistsBth });
                                //        }
                                //        else
                                //        {
                                //            if (aux || aux1)
                                //            {
                                //                var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                //                return RedirectToAction("Bamboo1", new { Outstock = 1, name = nombreProducto.Nombre, colorNombre = NoExistsBth });
                                //            }
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    if (pagi != null)
                                //    {
                                //        var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                //        return RedirectToAction("Tienda", new { Outstock = 1, pag = pagi, colorNombre = nombreProducto.Nombre });
                                //    }
                                //    else
                                //    {
                                //        var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                //        return RedirectToAction("Bamboo1", new { Outstock = 1, name = nombreProducto.Nombre, colorNombre = nombreProducto.Nombre });
                                //    }
                                //}

                            }
                            else
                            {
                                //if (carritoCantidad != null)
                                //{
                                    //var productoStock = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                    //var stockSuficiente = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona && x.idProducto == producto.Id).ToList();
                                    //var cantidadSuficiente = 0;
                                    //foreach (var item in stockSuficiente)
                                    //{
                                    //    cantidadSuficiente += item.Cantidad;
                                    //}

                                    //if (cantidadSuficiente + 1 <= productoStock.Cantidad )
                                    //{
                                    //if (carritoCantidad != null)
                                    //{
                                    //    if (carritoCantidad.Cantidad + cantidadProducto > inventarioColor.Cantidad)
                                    //    {

                                    //        if (pagi != null)
                                    //        {
                                    //            return RedirectToAction("Tienda", new { Outstock = 1, pag = pagi, colorNombre = Color.Nombre });
                                    //        }
                                    //        else
                                    //        {
                                    //            var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                    //            return RedirectToAction("Bamboo1", new { Outstock = 1, name = nombreProducto.Nombre, colorNombre = Color.Nombre });
                                    //        }

                                    //    }
                                    //}
                                    //}
                                    //else
                                    //{
                                    //    if (pagi != null)
                                    //    {
                                    //        var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                    //        return RedirectToAction("Tienda", new { Outstock = 1, pag = pagi, colorNombre = nombreProducto.Nombre });
                                    //    }
                                    //    else
                                    //    {
                                    //        var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                    //        return RedirectToAction("Bamboo1", new { Outstock = 1, name = nombreProducto.Nombre, colorNombre = nombreProducto.Nombre });
                                    //    }
                                    //}

                                   
                                //}
                            }

                        }


                    }

                    CarritoCompra agregarProductoCarrito = new CarritoCompra();
                    using (this.DbModel = new ApplicationDbContext())
                    {
                        var nombreProdcuto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                        var carritoList = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).ToList();
                        var carritofind = carritoList.FirstOrDefault(x => x.idProducto == producto.Id && x.ColorNombre == Color.Nombre && x.MaterialNombre == null);
                        var carritofindMaterialWColor = carritoList.FirstOrDefault(x => x.idProducto == producto.Id && x.ColorNombre == Color.Nombre && x.MaterialNombre == Material.Nombre);
                        var inventarioColor = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Color.Id == Color.Id);
                        var inventarioMaterialWColor = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Material.Id == Material.Id);
                        double precioTotal = 0.00;

                        if (Material.Id > 0)
                        {
                            if (carritofindMaterialWColor != null)
                            {
                                //if (carritofindMaterialWColor.Cantidad + 1 <= inventarioColor.Cantidad)
                                //{
                                    carritofindMaterialWColor.Cantidad += 1;
                                    carritofindMaterialWColor.StockProducto = nombreProdcuto.Cantidad;
                                    carritofindMaterialWColor.StockColor = inventarioColor.Cantidad;
                                    carritofindMaterialWColor.StockMaterial = inventarioMaterialWColor.Cantidad;
                                    precioTotal = (carritofindMaterialWColor.Cantidad * carritofindMaterialWColor.Precio);
                                    carritofindMaterialWColor.precioTotal = precioTotal;
                                    DbModel.CarritoCompras.AddOrUpdate(carritofindMaterialWColor);
                                    DbModel.SaveChanges();
                                //}
                            }
                            else
                            {
                                precioTotal = (nombreProdcuto.Precio + (double)Material.Precio);
                                agregarProductoCarrito.Nombre = nombreProdcuto.Nombre;
                                agregarProductoCarrito.Precio = precioTotal;
                                if (color != null && color != "")
                                {
                                    // var Color = DbModel.Colores.FirstOrDefault(x => x.Codigo == color);
                                    agregarProductoCarrito.ColorNombre = Color.Nombre;
                                    agregarProductoCarrito.CodigoColor = Color.Codigo;
                                    agregarProductoCarrito.idColor = Color.Id;
                                    agregarProductoCarrito.StockColor = inventarioColor.Cantidad;
                                    agregarProductoCarrito.Cantidad = cantidadProducto.Value;
                                }
                                else
                                {
                                    var ColorNatural = DbModel.Colores.FirstOrDefault(x => x.Nombre == "Natural");
                                    inventarioColor = null;
                                    inventarioColor = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Color.Id == ColorNatural.Id);
                                    //inventario = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Color.Id == 1);

                                    // 
                                    agregarProductoCarrito.ColorNombre = Color.Nombre;
                                    agregarProductoCarrito.CodigoColor = Color.Codigo;
                                    agregarProductoCarrito.idColor = Color.Id;
                                    agregarProductoCarrito.StockColor = inventarioColor.Cantidad;
                                    agregarProductoCarrito.Cantidad = cantidadProducto.Value;
                                }
                                
                                agregarProductoCarrito.Imagen = nombreProdcuto.Imagen;
                                agregarProductoCarrito.StockProducto = nombreProdcuto.Cantidad;
                                agregarProductoCarrito.StockMaterial = inventarioMaterialWColor.Cantidad;
                                agregarProductoCarrito.MaterialNombre = Material.Nombre;
                                agregarProductoCarrito.idMaterial = Material.Id;
                                agregarProductoCarrito.idProducto = producto.Id;
                                agregarProductoCarrito.SvgColor = SvgColor;
                                agregarProductoCarrito.idPersona = persona.idPersona;
                                agregarProductoCarrito.ImagenMaterial = Material.Imagen;
                                agregarProductoCarrito.precioTotal = agregarProductoCarrito.Cantidad * precioTotal;

                                DbModel.CarritoCompras.Add(agregarProductoCarrito);
                                DbModel.SaveChanges();
                            }
                        }
                        else
                        {
                            if (carritofind != null)
                            {
                                //if (carritofind.Cantidad + 1 <= inventarioColor.Cantidad)
                                //{
                                    carritofind.Cantidad += 1;
                                    carritofind.StockProducto = nombreProdcuto.Cantidad;
                                    carritofind.StockColor = inventarioColor.Cantidad;
                                    precioTotal = (carritofind.Cantidad * carritofind.Precio);
                                    carritofind.precioTotal = precioTotal;
                                    DbModel.CarritoCompras.AddOrUpdate(carritofind);
                                    DbModel.SaveChanges();
                                //}

                            }
                            else
                            {
                                agregarProductoCarrito.Nombre = nombreProdcuto.Nombre;
                                agregarProductoCarrito.Precio = nombreProdcuto.Precio;
                                if (color != null && color != "")
                                {
                                    // var Color = DbModel.Colores.FirstOrDefault(x => x.Codigo == color);
                                    agregarProductoCarrito.ColorNombre = Color.Nombre;
                                    agregarProductoCarrito.CodigoColor = Color.Codigo;
                                    agregarProductoCarrito.idColor = Color.Id;
                                    agregarProductoCarrito.StockColor = inventarioColor.Cantidad;
                                    agregarProductoCarrito.Cantidad = cantidadProducto.Value;
                                }
                                else
                                {
                                    var ColorNatural = DbModel.Colores.FirstOrDefault(x => x.Nombre == "Natural");
                                    inventarioColor = null;
                                    inventarioColor = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Color.Id == ColorNatural.Id);
                                    //inventario = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Color.Id == 1);

                                    // var Color = DbModel.Colores.FirstOrDefault(x => x.Id == 1);
                                    agregarProductoCarrito.ColorNombre = Color.Nombre;
                                    agregarProductoCarrito.CodigoColor = Color.Codigo;
                                    agregarProductoCarrito.idColor = Color.Id;
                                    agregarProductoCarrito.StockColor = inventarioColor.Cantidad;
                                    agregarProductoCarrito.Cantidad = cantidadProducto.Value;
                                }
                                agregarProductoCarrito.Imagen = nombreProdcuto.Imagen;
                                agregarProductoCarrito.idProducto = nombreProdcuto.Id;
                                //agregarProductoCarrito.MaterialNombre = null;
                                //agregarProductoCarrito.StockMaterial = 0;
                                agregarProductoCarrito.StockProducto = nombreProdcuto.Cantidad;
                                agregarProductoCarrito.SvgColor = SvgColor;
                                agregarProductoCarrito.idPersona = persona.idPersona;
                                precioTotal = (agregarProductoCarrito.Cantidad * agregarProductoCarrito.Precio);
                                agregarProductoCarrito.precioTotal = precioTotal;

                                DbModel.CarritoCompras.Add(agregarProductoCarrito);
                                DbModel.SaveChanges();

                            }
                        }

                    }


                    if (pagi != null)
                    {
                        return RedirectToAction("Tienda", new { pag = pagi });
                    }
                    else
                    {
                        if (esCarrito == "True")
                        {
                            return RedirectToAction("IndexTienda", "Tienda");
                        }
                        else
                        {
                            using (this.DbModel = new ApplicationDbContext())
                            {
                                var nombreProducto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                                return RedirectToAction("Bamboo1", new { name = nombreProducto.Nombre });
                            }
                        }

                    }

                }
                else if (accion == 2)
                {
                    // Eliminación
                    var productoCarrito = new CarritoCompra();
                    Color = DbModel.Colores.FirstOrDefault(x => x.Codigo == color);
                    var inventario = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == producto.Id && x.Color.Id == Color.Id);
                    double precioTotal = 0.00;

                    if (Material.Id > 0)
                    {
                        productoCarrito = DbModel.CarritoCompras.FirstOrDefault(x => x.idProducto == producto.Id && x.ColorNombre == Color.Nombre && x.MaterialNombre == Material.Nombre && x.idPersona == persona.idPersona);
                    }
                    else
                    {
                        productoCarrito = DbModel.CarritoCompras.FirstOrDefault(x => x.idProducto == producto.Id && x.ColorNombre == Color.Nombre && x.MaterialNombre == null && x.idPersona == persona.idPersona);

                    }

                    //var productoCarrito = DbModel.CarritoCompras.FirstOrDefault(x => x.CodigoColor == color && x.idProducto == producto.Id && x.idPersona == persona.idPersona);
                   

                    if (productoCarrito != null)
                    {
                        if (productoCarrito.Cantidad - 1 <= 0)
                        {
                            DbModel.CarritoCompras.Remove(productoCarrito);
                            DbModel.SaveChanges();
                        }
                        else
                        {

                            productoCarrito.Cantidad -= 1;
                            precioTotal = (productoCarrito.Cantidad * productoCarrito.Precio);
                            productoCarrito.precioTotal = precioTotal;
                            DbModel.CarritoCompras.AddOrUpdate(productoCarrito);
                            DbModel.SaveChanges();
                        }


                        if (pagi != null)
                        {
                            return RedirectToAction("Tienda", new { pag = pagi });
                        }
                        else
                        {
                            return RedirectToAction("IndexTienda", "Tienda");
                        }

                    }
                }
            }
            else
            {
                // El usuario no ha sido logeado, le mandamos un mensaje
                ViewBag.NoLogIn = 0;
                ViewBag.pagina = 1;
                Tienda(0, 1);
                return View("Tienda");
            }



            return View("Tienda");

        }
    }
}