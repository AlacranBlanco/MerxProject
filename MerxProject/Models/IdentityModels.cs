using System.Data.Entity;
using System.Drawing;
using System.Security.Claims;
using System.Threading.Tasks;
using MerxProject.Models;
using MerxProject.Models.Direccion;
using MerxProject.Models.CarritoCompras;
using MerxProject.Models.ProductosFavorito;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MerxProject.Models.Cupones;
using MerxProject.Models.Order;

namespace MerxProject.Models
{
    // Para agregar datos de perfil del usuario, agregue más propiedades a su clase ApplicationUser. Visite https://go.microsoft.com/fwlink/?LinkID=317594 para obtener más información.
    public class ApplicationUser : IdentityUser
    {

        public int idPersona { get; set; }

        public int IdUsuario { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Tenga en cuenta que el valor de authenticationType debe coincidir con el definido en CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Agregar aquí notificaciones personalizadas de usuario
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


        public DbSet<Persona> Personas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Color> Colores { get; set; }
        public DbSet<Mueble> Muebles { get; set; }
        public DbSet<Material> Materiales { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetalleCompra { get; set; }
        public DbSet<Herramienta> Herramientas { get; set; }
        public DbSet<Direcciones> Direcciones { get; set; }

        public DbSet<Paises> Paises { get; set; }

        public DbSet<ProductosFavoritos> ProductosFavoritos { get; set; }

        public DbSet<CarritoCompra> CarritoCompras { get; set; }

        public DbSet<Cupon> Cupon { get; set; }


        public DbSet<Orders> Orders { get; set; }

        public DbSet<OrdersDetails> OrdersDetails { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Compra >()
                .Ignore(e => e.DS_Estatus);

            modelBuilder.Entity<DetalleCompra>()
                .Ignore(e => e.Tipo);

            /*modelBuilder.Entity<Compra>()
                .HasMany(e => e.DetalleCompra)
                .WithOptional(e => e.Compra)
                .HasForeignKey(e => e.IdCompra);

            modelBuilder.Entity<Proveedor>()
                .HasMany(e => e.Compras)
                .WithOptional(e => e.Proveedor)
                .HasForeignKey(e => e.IdProveedor);

            modelBuilder.Entity<Empleado>()
                .HasMany(e => e.Compras)
                .WithOptional(e => e.Empleado)
                .HasForeignKey(e => e.IdEmpleado);*/

            /*modelBuilder.Entity<Material>()
                .HasMany(e => e.DetalleCompras)
                .WithOptional(e => e.MateriaPrima)
                .HasForeignKey(e => e.MateriaPrima);

            modelBuilder.Entity<Herramienta>()
                .HasMany(e => e.DetalleCompras)
                .WithOptional(e => e.Herramienta)
                .HasForeignKey(e => e.Herramienta);*/
        }
    }
}