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
    [Authorize(Roles = "Administrador, Empleado")]
    public class MaterialController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Material> _Materiales;
        private PaginadorGenerico<Material> _PaginadorMateriales;

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpMateriales(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (accion == "1")
                {
                    var Material = new Material();
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View(Material);
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
                    materiales.Piezas = material.Piezas;

                    if (material != null)
                    {
                        if(postedFile != null)
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
                        }
                        else
                        {
                            materiales.Imagen = material.Imagen;
                        }
                        try
                        {
                            if (material.Nombre == "Barniz" || material.Nombre == "Barniz Bambú")
                            {
                                materiales.Nombre = material.Nombre;
                            }
                            DbModel.Materiales.AddOrUpdate(materiales);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMaterial");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
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
                            if(material.Nombre == "Barniz" || material.Nombre == "Barniz Bambú" || material.Nombre.Contains("Pintura"))
                            {
                                Session["res"] = "Este material es esencial para el funcionamiento de la página, no se puede eliminar";
                                return RedirectToAction("ListaMaterial");
                            }
                            var producto = DbModel.Productos.Where(x => x.CategoriaMaterial.Id == material.Id).Count();
                            
                            if (producto < 1)
                            {
                                var procesosActuales = DbModel.Procesos.Where(x => x.Inventario.Producto.CategoriaMaterial.Id == material.Id).Count();
                                if (procesosActuales < 1)
                                {
                                    if (material.Piezas)
                                    {
                                        var piezas = DbModel.PiezaMateriales.Where(x => x.Material.Id == material.Id).ToList();
                                        DbModel.PiezaMateriales.RemoveRange(piezas);
                                    }
                                    DbModel.Materiales.Remove(material);
                                    DbModel.SaveChanges();
                                    resultado = "Eliminación finalizada";
                                    Session["res"] = resultado;
                                    Session["tipo"] = "Exito";
                                    return RedirectToAction("ListaMaterial");
                                }
                                else
                                {
                                    resultado = "Un producto de este tipo está en proceso. No se puede eliminar";
                                    Session["res"] = resultado;
                                    return RedirectToAction("ListaMaterial");
                                }
                            }
                            else
                            {
                                resultado = "Aún hay productos con este material, no se puede eliminar";
                                Session["res"] = resultado;
                                return RedirectToAction("ListaMaterial");
                            }
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
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
                            var Piezas = new List<PiezaMaterial>();

                            if (materiales.Piezas)
                            {
                                PiezaMaterial pieza1 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Viga").First()
                                };
                                Piezas.Add(pieza1);
                                PiezaMaterial pieza2 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Tablón").First()
                                };
                                Piezas.Add(pieza2);
                                PiezaMaterial pieza3 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Tabla").First()
                                };
                                Piezas.Add(pieza3);
                                PiezaMaterial pieza4 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Tarima").First()
                                };
                                Piezas.Add(pieza4);
                                PiezaMaterial pieza5 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Lata").First()
                                };
                                Piezas.Add(pieza5);
                                PiezaMaterial pieza6 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Regruesos").First()
                                };
                                Piezas.Add(pieza6);
                                PiezaMaterial pieza7 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Chapas").First()
                                };
                                Piezas.Add(pieza7);
                                PiezaMaterial pieza8 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Vigueta").First()
                                };
                                Piezas.Add(pieza8);
                                PiezaMaterial pieza9 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Alfarjía").First()
                                };
                                Piezas.Add(pieza9);
                                PiezaMaterial pieza10 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Listones").First()
                                };
                                Piezas.Add(pieza10);
                                PiezaMaterial pieza11 = new PiezaMaterial()
                                {
                                    Cantidad = 0,
                                    Material = materiales,
                                    Pieza = DbModel.Piezas.Where(x => x.Nombre == "Listoncillos").First()
                                };
                                Piezas.Add(pieza11);
                            }

                            DbModel.Materiales.Add(materiales);
                            if (Piezas != null)
                            {
                                DbModel.PiezaMateriales.AddRange(Piezas);
                            }
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMaterial");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
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
        [Authorize(Roles ="Administrador, Empleado")]
        public ActionResult ListaMaterial(int pagina = 1)
        {
            
            int _TotalRegistros = 0;
            
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var mat = DbModel.Materiales.Where(x => x.Nombre == "Barniz Bambú").Count();
                if(mat < 1)
                {
                    var BarnizEspecial = new Material()
                    {
                        Cantidad = 0,
                        Descripcion = "Barniz especial para Bambú, también sirve para para mimbres, paja y palma",
                        Medida = "Galones",
                        Nombre = "Barniz Bambú",
                        Piezas = false,
                        Precio = 0
                    };
                    DbModel.Materiales.Add(BarnizEspecial);
                    DbModel.SaveChanges();
                }
                var mat1 = DbModel.Materiales.Where(x => x.Nombre == "Barniz Bambú").Count();
                if (mat < 1)
                {
                    var BarnizEspecial = new Material()
                    {
                        Cantidad = 0,
                        Descripcion = "Un acabado o película protectora transparente",
                        Medida = "Galones",
                        Nombre = "Barniz",
                        Piezas = false,
                        Precio = 0
                    };
                    DbModel.Materiales.Add(BarnizEspecial);
                    DbModel.SaveChanges();
                }
                

                var count = DbModel.Piezas.Count();
                if (count < 1)
                {
                    var Piezas = new List<Pieza>();
                    Pieza pieza1 = new Pieza()
                    {
                        Nombre = "Viga"
                    };
                    Piezas.Add(pieza1);
                    Pieza pieza2 = new Pieza()
                    {
                        Nombre = "Tablón"
                    };
                    Piezas.Add(pieza2);
                    Pieza pieza3 = new Pieza()
                    {
                        Nombre = "Tabla"
                    };
                    Piezas.Add(pieza3);
                    Pieza pieza4 = new Pieza()
                    {
                        Nombre = "Tarima"
                    };
                    Piezas.Add(pieza4);
                    Pieza pieza5 = new Pieza()
                    {
                        Nombre = "Lata"
                    };
                    Piezas.Add(pieza5);
                    Pieza pieza6 = new Pieza()
                    {
                        Nombre = "Regruesos"
                    };
                    Piezas.Add(pieza6);
                    Pieza pieza7 = new Pieza()
                    {
                        Nombre = "Chapas"
                    };
                    Piezas.Add(pieza7);
                    Pieza pieza8 = new Pieza()
                    {
                        Nombre = "Vigueta"
                    };
                    Piezas.Add(pieza8);
                    Pieza pieza9 = new Pieza()
                    {
                        Nombre = "Alfarjía"
                    };
                    Piezas.Add(pieza9);
                    Pieza pieza10 = new Pieza()
                    {
                        Nombre = "Listones"
                    };
                    Piezas.Add(pieza10);
                    Pieza pieza11 = new Pieza()
                    {
                        Nombre = "Listoncillos"
                    };
                    Piezas.Add(pieza11);
                    DbModel.Piezas.AddRange(Piezas);
                    DbModel.SaveChanges();
                }
                    // Número total de registros de la tabla Productos
                    _TotalRegistros = DbModel.Materiales.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Materiales = DbModel.Materiales.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMateriales = new PaginadorGenerico<Material>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Materiales
                };


                return View(_PaginadorMateriales);
            }
        }

        public ActionResult BuscarLista(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Materiales.Where(x => x.Nombre.Contains(parameter) ||
                                                          (x.Descripcion.Contains(parameter))).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Materiales = DbModel.Materiales.Where(x => x.Nombre.Contains(parameter) ||
                                                      (x.Descripcion.Contains(parameter))).OrderBy(x => x.Nombre)
                                                      .Skip((pagina - 1) * _RegistrosPorPagina)
                                                      .Take(_RegistrosPorPagina)
                                                      .ToList();
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMateriales = new PaginadorGenerico<Material>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Materiales
                };

                if (_TotalRegistros < 1)
                {
                    Session["res"] = ("No hay resultados");
                    return RedirectToAction("ListaMaterial");
                }
                else
                {
                    return View("ListaMaterial", _PaginadorMateriales);
                }
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpInventario(int Id)
        {
            if (User.IsInRole("Administrador"))
            {
                ViewBag.Admin = true;
            }

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (User.IsInRole("Administrador"))
                {
                    ViewBag.Admin = true;
                }


                var material = DbModel.Materiales.Find(Id);
                var Piezas = DbModel.Piezas.ToList();
                var piezas = DbModel.PiezaMateriales.Where(x=>x.Material.Id == material.Id).ToList();

                ViewBag.title = material.Nombre;
                ViewBag.max = material.Cantidad;
                ViewBag.materialId = material.Id;
                ViewBag.invId = piezas;
                


                return View(piezas);
                //var material = DbModel.Materiales.Find(Id);
                //var piezas = DbModel.Piezas.Where(x => x.Material.Id == Id).Count();
                //if(piezas > 0)
                //{
                //    var PiezasActuales = DbModel.Piezas.Where(x => x.Material.Id == Id).ToList();
                //    return View(PiezasActuales);
                //}
                //else
                //{
                //    ViewBag.Material = Id;
                //    return View();
                //}
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult popUpInventario(int Material, int[] Id, int[] Cantidad, int cant, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var material = DbModel.Materiales.Find(Material);
                try
                {
                    if (accion == "Agregar")
                    {
                        for (int i = 0; i < Id.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(Cantidad[i].ToString()))
                            {
                                var pieza = DbModel.PiezaMateriales.Find(Id[i]);
                                if (pieza != null)
                                {
                                    pieza.Cantidad += Cantidad[i];
                                    DbModel.PiezaMateriales.AddOrUpdate(pieza);
                                }
                                else
                                {
                                    DbModel.PiezaMateriales.Add(pieza);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Id.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(Cantidad[i].ToString()))
                            {
                                var pieza = DbModel.PiezaMateriales.Find(Id[i]);
                                if (pieza != null)
                                {
                                    pieza.Cantidad = Cantidad[i];
                                    DbModel.PiezaMateriales.AddOrUpdate(pieza);
                                }
                                else
                                {
                                    DbModel.PiezaMateriales.Add(pieza);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    material.Cantidad -= cant;
                    DbModel.Materiales.AddOrUpdate(material);
                    DbModel.SaveChanges();
                    Session["res"] = "Actualización realizada";
                    Session["tipo"] = "Exito";
                    return RedirectToAction("ListaMaterial");
                }
                catch(Exception ex)
                {
                    Session["res"] = ex.Message;
                    return RedirectToAction("ListaMaterial");
                }
            }
        }

    }
}
