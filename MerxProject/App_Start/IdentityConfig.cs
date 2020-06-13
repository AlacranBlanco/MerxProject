﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using MerxProject.Models;
using System.Net.Mail;
using System.Net.Mime;
using System.Configuration;

namespace MerxProject
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Conecte su servicio de correo electrónico aquí para enviar correo electrónico.
            //return Task.FromResult(0);
            return Task.Factory.StartNew(() =>
            {
                sendMail(message);
            });
        }

        void sendMail(IdentityMessage message)
        {
            #region formatter
            string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);

           // string html = HttpUtility.HtmlEncode(@"Or click on the copy the following link on the browser:" + message.Body);
           string msgChido = "<html><table border=" + "0" + " width=" + "100%" + " cellspacing=" + "0" + " cellpadding=" + "0" + " bgcolor=" + "#FFFFFF" + "><tbody><tr><td style=" + "min-width:722px" + " align=" + "center" + "><table border=" + "0" + " width=" + "722" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody><tr><td style=" + "line-height:0px" + " bgcolor=" + "#F7F8F8" + " height=" + "2" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "2" + " class=" + "CToWUd" + "></td></tr><tr><td><table border=" + "0" + " width=" + "722" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody><tr><td bgcolor=" + "#F7F8F8" + " width=" + "2" + ">&nbsp;</td><td align=" + "center" + " width=" + "718" + "><table border=" + "0" + " width=" + "100%" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody></tbody></table><br><table border=" + "0" + " width=" + "65%" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody></tbody></table><table border=" + "0" + " width=" + "65%" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody><tr><td style=" + "line-height:0px" + " height=" + "20" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "1" + " class=" + "CToWUd" + "></td></tr><tr><td style=" + "font-family:Verdana,Geneva,sans-serif;font-size:13px;line-height:21px;color:#3d454d" + " align=" + "left" + " valign=" + "top" + ">Te damos la bienvenida a Merx Company. En nuestro sitio web podrás encontrar todo lo necesario para comenzar a realizar tus compras, desde varios tipos de estilos ya sean para interiores o exteriores.</td></tr><tr><td style=" + "line-height:0px" + " height=" + "20" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "1" + " class=" + "CToWUd" + "></td></tr><tr><td style=" + "font-family:Verdana,Geneva,sans-serif;font-size:13px;line-height:21px;color:#3d454d" + " align=" + "left" + " valign=" + "top" + "><p style=" + "color:#3d454d;font-family:Verdana,Geneva,sans-serif;font-size:13px;line-height:21px" + ">Por último, debes verificar que la cuenta de correo electrónico que has adherido sea válida y se encuentre activa. Abajo te dejamos un enlace de confirmación para que puedas confirmar este paso.</p></td></tr><tr><td style=" + "line-height:0px" + " height=" + "20" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "1" + " class=" + "CToWUd" + "></td></tr><tr><td style=" + "text-align:center" + "><table border=" + "0" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody><tr><td style=" + "border-radius:3px;font:13px Verdana,Geneva,sans-serif" + " align=" + "center" + " bgcolor=" + "#007EE5" + "><a style=" + "text-decoration:none;color:#fff;display:block;padding:12px 40px 14px" + " href=" + message.Body + " target=" + "_blank" + " data-saferedirecturl=" + "https://www.google.com/url?q=" + message.Body +  ">Activar cuenta</a></td></tr><tr><td style=" + "line-height:0px" + " height=" + "20" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "1" + " class=" + "CToWUd" + "></td></tr></tbody></table></td></tr></tbody></table><table style=" + "margin-left:auto;margin-right:auto" + " border=" + "0" + " width=" + "65%" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody><tr><td style=" + "font-family:Verdana,Geneva,sans-serif;font-size:13px;line-height:21px;color:#3d454d" + " align=" + "left" + " valign=" + "top" + "><p style=" + "color:#3d454d;font-family:Verdana,Geneva,sans-serif;font-size:13px;line-height:21px" + "><span style=" + "color:#3d454d;font-family:Verdana,Geneva,sans-serif;font-size:13px;font-style:normal;font-variant:normal;font-weight:normal;letter-spacing:normal;line-height:21px;text-align:-webkit-left;text-indent:0px;text-transform:none;white-space:normal;word-spacing:0px;display:inline!important;float:none;background-color:#ffffff" + ">En caso de que usted no haya solicitado ningún tipo de confirmación asociada a Merx Company, le rogamos que ignore esta notificación.</span></p><span style=" + "color:#3d454d;font-family:Verdana,Geneva,sans-serif;font-size:13px;font-style:normal;font-variant:normal;font-weight:normal;letter-spacing:normal;line-height:21px;text-align:-webkit-left;text-indent:0px;text-transform:none;white-space:normal;word-spacing:0px;display:inline!important;float:none;background-color:#ffffff" + "><br>Un saludo,&nbsp;<br>El equipo de Merx Company</span></td></tr><tr><td style=" + "line-height:0px" + " height=" + "30" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "1" + " class=" + "CToWUd" + "></td></tr></tbody></table><table style=" + "margin-left:auto;margin-right:auto" + " border=" + "0" + " width=" + "100%" + " cellspacing=" + "0" + " cellpadding=" + "0" + "><tbody><tr><td style=" + "padding:20px 22px;border-collapse:collapse" + " bgcolor=" + "#F7F8F8" + "><table border=" + "0" + " cellspacing=" + "0" + " cellpadding=" + "0" + " align=" + "left" + "><tbody><tr><td align=" + "left" + " width=" + "78" + " height=" + "30" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "1" + " class=" + "CToWUd" + "></td></tr></tbody></table><table border=" + "0" + " cellspacing=" + "0" + " cellpadding=" + "0" + " align=" + "left" + "><tbody><tr><td><table border=" + "0" + " cellspacing=" + "0" + " cellpadding=" + "0" + " align=" + "left" + "><tbody><tr><td align=" + "center" + " width=" + "68" + " height=" + "30" + "><a href=" + "no hay feis we" + " target=" + "_blank" + " data-saferedirecturl=" + "no" + "><img style=" + "display:block;border:0" + " src=" + "https://ci6.googleusercontent.com/proxy/BBvij0O2WnB5J0vhQLunhcV0-rDGLumlXNMHKF0RrG1Djl5xBrAv-KC-qQy1Akd10gOg-sk_3IxScwd6gfEpF98W7Q=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/fb.png" + " alt=" + "facebook" + " width=" + "20" + " height=" + "18" + " class=" + "CToWUd" + "></a></td><td bgcolor=" + "#BDC1C2" + " width=" + "1" + ">&nbsp;</td><td align=" + "center" + " width=" + "68" + "><a href=" + "NA" + " target=" + "_blank" + " data-saferedirecturl=" + "https://www.google.com/url?q=https://twitter.com/LSRPes&amp;source=gmail&amp;ust=1591758210973000&amp;usg=AFQjCNHlYRXYjEnhS8-1yvcql0wrPDVOyQ" + "><img style=" + "display:block;border:0" + " src=" + "NA" + " alt=" + "twitter" + " width=" + "20" + " height=" + "19" + " class=" + "CToWUd" + "></a></td><td bgcolor=" + "#BDC1C2" + " width=" + "1" + ">&nbsp;</td></tr></tbody></table></td></tr></tbody></table><table border=" + "0" + " cellspacing=" + "0" + " cellpadding=" + "0" + " align=" + "left" + "><tbody><tr><td style=" + "font-family:Verdana,Geneva,sans-serif;font-size:12px;line-height:14px;color:#adb1b4;padding-left:25px;padding-right:25px" + " align=" + "center" + " height=" + "30" + ">© 2020 Merx Company</td></tr></tbody></table></td></tr></tbody></table></td><td bgcolor=" + "#F7F8F8" + " width=" + "1" + ">&nbsp;</td></tr></tbody></table></td></tr><tr><td style=" + "line-height:0px" + " bgcolor=" + "#F7F8F8" + " height=" + "2" + "><img style=" + "display:block;border:0" + " src=" + "https://ci5.googleusercontent.com/proxy/aZzjx0tY5tGjrfPrvCaVjLb3aZAsKpwgx8hYkugIvux5wiPEQbyMmKTqFKFrspQQCxUJa1FHiHxY17UBoO-SP-jSBsj6hLI=s0-d-e1-ft#https://files.ls-rp.ovh/static/img/emails/spacer.gif" + " alt=" + "" + " width=" + "1" + " height=" + "1" + " class=" + "CToWUd" + "></td></tr></tbody></table></td></tr></tbody></table></html>";
            #endregion

            MailMessage msg = new MailMessage();
			
			msg.IsBodyHtml = true;
            msg.From = new MailAddress(ConfigurationManager.AppSettings["Email"].ToString());
            msg.To.Add(new MailAddress(message.Destination));
            msg.Subject = message.Subject;
			msg.Body = msgChido;
           // msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Email"].ToString(), ConfigurationManager.AppSettings["Password"].ToString());
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
        }

    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Conecte el servicio SMS aquí para enviar un mensaje de texto.
            return Task.FromResult(0);
        }
    }

    // Configure el administrador de usuarios de aplicación que se usa en esta aplicación. UserManager se define en ASP.NET Identity y se usa en la aplicación.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure la lógica de validación de nombres de usuario
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure la lógica de validación de contraseñas
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configurar valores predeterminados para bloqueo de usuario
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Registre proveedores de autenticación en dos fases. Esta aplicación usa los pasos Teléfono y Correo electrónico para recibir un código para comprobar el usuario
            // Puede escribir su propio proveedor y conectarlo aquí.
            manager.RegisterTwoFactorProvider("Código telefónico", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Su código de seguridad es {0}"
            });
            manager.RegisterTwoFactorProvider("Código de correo electrónico", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Código de seguridad",
                BodyFormat = "Su código de seguridad es {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure el administrador de inicios de sesión que se usa en esta aplicación.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
