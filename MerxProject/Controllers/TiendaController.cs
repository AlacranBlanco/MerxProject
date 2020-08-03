using MerxProject.Models;
using MerxProject.Models.CarritoCompras;
using MerxProject.Models.Direccion;
using MerxProject.Models.Order;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    public class TiendaController : Controller
    {
        ApplicationDbContext DbModel;

        public TiendaController()
        {
            this.DbModel = new ApplicationDbContext();
        }
        // GET: Tienda
        public ActionResult IndexTienda(int? outStock)
        {

            if (outStock != null)
            {
                ViewBag.outStock = 1;
            }
            else
            {
                ViewBag.outStock = 0;
            }

            using (this.DbModel = new ApplicationDbContext())
            {
                var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);

                var model = new CarritoComprasViewModel
                {
                    CarritoCollection = DbModel.CarritoCompras.Where(x => x.idPersona == idPersona.idPersona).OrderBy(x => x.ColorNombre).DistinctBy(x => x.ColorNombre).ToList(),
                    CarritoCollectionRepetido = DbModel.CarritoCompras.Where(x => x.idPersona == idPersona.idPersona).OrderBy(x => x.ColorNombre).ToList()
                };

                model.CantidadesProductos = new List<CarritoCompra>();
                int counts = 0;
                float precioTotal = 0;
                int j = 0, i = 0;

                while (i < model.CarritoCollection.Count)
                {
                    CarritoCompra carrito = new CarritoCompra();
                    if (j < model.CarritoCollectionRepetido.Count)
                    {
                        if (model.CarritoCollectionRepetido[j].ColorNombre == model.CarritoCollection[i].ColorNombre)
                        {
                            counts++;
                            precioTotal += model.CarritoCollectionRepetido[i].Precio;
                            j++;
                        }
                        else
                        {
                            carrito.Cantidad = counts;
                            carrito.Precio = precioTotal;
                            model.CantidadesProductos.Add(carrito);
                            counts = 0;
                            precioTotal = 0;
                            i++;

                        }
                    }
                    else
                    {
                        carrito.Cantidad = counts;
                        carrito.Precio = precioTotal;
                        model.CantidadesProductos.Add(carrito);
                        counts = 0;
                        precioTotal = 0;
                        i++;
                    }

                }

                var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
                var cupons = DbModel.Cupon.FirstOrDefault(x => x.IdPersona == persona.idPersona);

                if (cupons != null)
                {
                    ViewBag.descuentoGuardado = cupons.Descuento / 100.00;
                }

                return View(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> IndexTienda(string cupon, string ship, int subtotal)
        {


            var cupons = DbModel.Cupon.FirstOrDefault(x => x.CodigoCupon == cupon);
            if (cupons != null)
            {
                double descuento = (cupons.Descuento / 100.00);

                var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
                cupons.IdPersona = idPersona.idPersona;
                cupons.Utilizado = true;

                using (this.DbModel = new ApplicationDbContext())
                {

                    MerxProject.Models.Order.Orders order = new Models.Order.Orders()
                    {
                        NombreCustomer = "Nueva",
                        NumeroCustomer = "",
                        DireccionCustomer = "",
                        EmailCustomer = "",
                        idPersona = idPersona.idPersona,
                        DiaOrden = DateTime.Now,
                        TipoPago = "",
                        Estatus = ""

                    };

                    DbModel.Orders.Add(order);
                    DbModel.SaveChanges();


                    cupons.idOrder = order.IdOrder;
                    DbModel.Cupon.AddOrUpdate(cupons);
                    DbModel.SaveChanges();
                }

                ViewBag.radioBtn = ship;
                ViewBag.subTotalDesc = subtotal * descuento;
                IndexTienda(null);
                return View();
            }
            else
            {
                ViewBag.CupNotExit = 1;
                IndexTienda(null);
                return View();
            }



        }

        [HttpGet]
        public async Task<ActionResult> PagoProducto(string subtotal, string total, string cupon, string ship)
        {

            using (this.DbModel = new ApplicationDbContext())
            {
                var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);


                var fromDatabaseEF = new SelectList(DbModel.Direcciones.Where(x => x.IdPersona == persona.idPersona).ToList(), "Id", "DirCalle");


                if (ship != "Free")
                {

                    char[] shipAux = ship.ToCharArray();
                    ship = "";
                    for (int i = 0; i < shipAux.Length; i++)
                    {
                        if (shipAux[i] != '$')
                        {
                            ship += shipAux[i];
                        }

                    }


                    double ships = Convert.ToDouble(ship);
                    ViewBag.MySkills = fromDatabaseEF;
                    ViewBag.subtotal = subtotal;
                    ViewBag.total = (Convert.ToInt32(total) + Convert.ToInt32(ships));
                    ViewBag.cupon = cupon;
                    ViewBag.ship = ship;
                }
                else
                {
                    ViewBag.MySkills = fromDatabaseEF;
                    ViewBag.subtotal = subtotal;
                    ViewBag.total = total;
                    ViewBag.cupon = cupon;
                    ViewBag.ship = ship;
                }

                return View();
            }

        }

        [HttpPost]
        public async Task<ActionResult> PagoProducto(Direcciones direcciones, string subtotal, string total, string cupon, string ship)
        {

            using (this.DbModel = new ApplicationDbContext())
            {
                var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
                var direccion = DbModel.Direcciones.FirstOrDefault(x => x.Id == direcciones.Id);

                var fromDatabaseEF = new SelectList(DbModel.Direcciones.Where(x => x.IdPersona == persona.idPersona).ToList(), "Id", "DirCalle");


                ViewBag.direccion = direccion.Id;
                ViewBag.MySkills = fromDatabaseEF;
                ViewBag.subtotal = subtotal;
                ViewBag.total = total;
                ViewBag.cupon = cupon;
                ViewBag.ship = ship;



                return View(direccion);
            }

        }

        public ActionResult ProcessOrder(int direccion)
        {

            var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);

            var orderFind = DbModel.Orders.FirstOrDefault(x => x.idPersona == persona.idPersona && x.NombreCustomer == "Nueva");

            var datosCustomer = DbModel.Direcciones.FirstOrDefault(x => x.Id == direccion);
            int idOrder = 0;

            if (orderFind != null)
            {
                idOrder = orderFind.IdOrder;
                using (this.DbModel = new ApplicationDbContext())
                {

                    MerxProject.Models.Order.Orders order = new Models.Order.Orders()
                    {
                        IdOrder = orderFind.IdOrder,
                        NombreCustomer = datosCustomer.NombreCompleto,
                        NumeroCustomer = datosCustomer.NumTelefono,
                        DireccionCustomer = datosCustomer.DirCalle + ", " + datosCustomer.CodigoPostal + ", " + datosCustomer.Estado + ", " + datosCustomer.Ciudad + ", " + datosCustomer.Asentamiento,
                        EmailCustomer = User.Identity.Name,
                        idPersona = persona.idPersona,
                        DiaOrden = DateTime.Now,
                        TipoPago = "Pago Online",
                        Estatus = "Procesando"
                    };

                    DbModel.Orders.AddOrUpdate(order);
                    DbModel.SaveChanges();

                }
            }
            else
            {

                using (this.DbModel = new ApplicationDbContext())
                {

                    MerxProject.Models.Order.Orders order = new Models.Order.Orders()
                    {
                        NombreCustomer = datosCustomer.NombreCompleto,
                        NumeroCustomer = datosCustomer.NumTelefono,
                        DireccionCustomer = datosCustomer.DirCalle + ", " + datosCustomer.CodigoPostal + ", " + datosCustomer.Estado + ", " + datosCustomer.Ciudad + ", " + datosCustomer.Asentamiento,
                        EmailCustomer = User.Identity.Name,
                        idPersona = persona.idPersona,
                        DiaOrden = DateTime.Now,
                        TipoPago = "Pago Online",
                        Estatus = "Procesando"
                    };

                    DbModel.Orders.Add(order);
                    DbModel.SaveChanges();
                    idOrder = order.IdOrder;
                }
            }


            using (this.DbModel = new ApplicationDbContext())
            {

                var model = new CarritoComprasViewModel
                {
                    CarritoCollection = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).OrderBy(x => x.ColorNombre).DistinctBy(x => x.ColorNombre).ToList(),
                    CarritoCollectionRepetido = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).OrderBy(x => x.ColorNombre).ToList()
                };

                model.CantidadesProductos = new List<CarritoCompra>();
                int counts = 0;
                float precioTotal = 0;
                int j = 0, i = 0;

                while (i < model.CarritoCollection.Count)
                {
                    CarritoCompra carrito = new CarritoCompra();
                    if (j < model.CarritoCollectionRepetido.Count)
                    {
                        if (model.CarritoCollectionRepetido[j].ColorNombre == model.CarritoCollection[i].ColorNombre)
                        {
                            counts++;
                            precioTotal += model.CarritoCollectionRepetido[i].Precio;
                            j++;
                        }
                        else
                        {
                            carrito.Cantidad = counts;
                            carrito.Precio = precioTotal;
                            model.CantidadesProductos.Add(carrito);
                            counts = 0;
                            precioTotal = 0;
                            i++;

                        }
                    }
                    else
                    {
                        carrito.Cantidad = counts;
                        carrito.Precio = precioTotal;
                        model.CantidadesProductos.Add(carrito);
                        counts = 0;
                        precioTotal = 0;
                        i++;
                    }

                }

                i = 0;
                foreach (var item in model.CarritoCollection)
                {
                    OrdersDetails ordersDetails = new OrdersDetails()
                    {
                        idOrder = idOrder,
                        idProducto = item.idProducto,
                        Cantidad = model.CantidadesProductos[i].Cantidad,
                        Precio = item.Precio
                    };
                    i++;

                    DbModel.OrdersDetails.Add(ordersDetails);
                    DbModel.SaveChanges();
                }




            }

            return View();

        }

        private Payment payment;


        public ActionResult PagoConPaypal(double? totalPagoPyapal, string totalship, string Cancel = null)
        {
            //getting the apiContext
            APIContext apiContext = PaypalConfiguration.GetAPIContext();

            try
            {
                string payerId = Request.Params["PayerID"];
                totalPagoPyapal -= Convert.ToInt32(totalship);
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Tienda/PagoConPaypal?";
                    var guid = Convert.ToString((new Random()).Next(100000000));
                    var CrearPago = this.CrearPago(apiContext, baseURI + "guid=" + guid, totalPagoPyapal);

                    var links = CrearPago.links.GetEnumerator();
                    string paypalRedirectURL = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectURL = link.href;
                        }
                    }

                    Session.Add(guid, CrearPago.id);
                    return Redirect(paypalRedirectURL);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("Failure");
                    }
                }
            }
            catch (Exception ex)
            {

                PaypalLogger.Log("Error" + ex.Message);
                return View("Failure");
            }
            return View("Success");
        }

        private Payment CrearPago(APIContext apiContext, string redirectUrl, double? totalPagoPyapal)
        {

            var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
            //create itemlist and add item objects to it
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };

            var model = new CarritoComprasViewModel
            {
                CarritoCollection = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).OrderBy(x => x.ColorNombre).DistinctBy(x => x.ColorNombre).ToList(),
                CarritoCollectionRepetido = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).OrderBy(x => x.ColorNombre).ToList()
            };

            model.CantidadesProductos = new List<CarritoCompra>();
            int counts = 0;
            float precioTotal = 0;
            int j = 0, i = 0;

            while (i < model.CarritoCollection.Count)
            {
                CarritoCompra carrito = new CarritoCompra();
                if (j < model.CarritoCollectionRepetido.Count)
                {
                    if (model.CarritoCollectionRepetido[j].ColorNombre == model.CarritoCollection[i].ColorNombre)
                    {
                        counts++;
                        precioTotal += model.CarritoCollectionRepetido[i].Precio;
                        j++;
                    }
                    else
                    {
                        carrito.Cantidad = counts;
                        carrito.Precio = precioTotal;
                        model.CantidadesProductos.Add(carrito);
                        counts = 0;
                        precioTotal = 0;
                        i++;

                    }
                }
                else
                {
                    carrito.Cantidad = counts;
                    carrito.Precio = precioTotal;
                    model.CantidadesProductos.Add(carrito);
                    counts = 0;
                    precioTotal = 0;
                    i++;
                }

            }



            i = 0;

            var cuponUsado = DbModel.Cupon.Where(x => x.IdPersona == persona.idPersona).OrderBy(x => x.idOrder).ToList();

            double descuento = 0.00;

            if (cuponUsado[0] != null)
            {
                descuento = cuponUsado[0].Descuento / 100.00;

                foreach (var item in model.CarritoCollection)
                {
                    int DescAns = Convert.ToInt32((Convert.ToDouble(item.Precio) * descuento));
                    itemList.items.Add(new Item()
                    {
                        quantity = model.CantidadesProductos[i].Cantidad.ToString(),
                        name = item.Nombre,
                        price = DescAns.ToString(),
                        currency = "MXN",
                    });
                    i++;
                }
            }
            else
            {
                foreach (var item in model.CarritoCollection)
                {
                    itemList.items.Add(new Item()
                    {
                        quantity = model.CantidadesProductos[i].Cantidad.ToString(),
                        name = item.Nombre,
                        price = item.Precio.ToString(),
                        currency = "MXN",
                    });
                    i++;
                }
            }



            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = totalPagoPyapal.ToString()
            };
            //Final amount with details
            var amount = new Amount()
            {
                currency = "MXN",
                total = (Convert.ToInt32(details.tax) + Convert.ToInt32(details.shipping) + Convert.ToInt32(details.subtotal)).ToString(), // Total must be equal to sum of tax, shipping and subtotal.
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = "your generated invoice number", //Generate an Invoice No
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);

        }



        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }



    }

}
