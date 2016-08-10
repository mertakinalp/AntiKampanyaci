using AntiKampanyaci.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AntiKampanyaci.Controllers
{
    public class BaseController : Controller
    {
        //
        // GET: /Base/

        AkEntities ent = new AkEntities();

        public void SaveLoginAttempt(string FacebookId,string FacebookName, string IP, bool isAdmin, string UserAgent) 
        {
            tblLogin loginAttempt = new tblLogin();

            loginAttempt.FacebookId = FacebookId;
            loginAttempt.FacebookName = FacebookName;
            loginAttempt.IP = IP;
            loginAttempt.isAdmin = isAdmin;
            loginAttempt.LoginDate = DateTime.Now.Date;
            loginAttempt.LoginDatetime = DateTime.Now;
            loginAttempt.UserAgent = UserAgent;

           // ent.Entry(loginAttempt).State = System.Data.EntityState.Added;

            ent.tblLogins.Add(loginAttempt);

            ent.SaveChanges();
        }

        public bool isBanned(string FacebookId) 
        {
            bool Banned = false;

            if (string.IsNullOrWhiteSpace(FacebookId))
            {
                return Banned;
            }

            int BannedCount = 0;

            BannedCount = ent.tblBans.Where(w => w.FacebookId == FacebookId).Count();

            if (BannedCount > 0)
            {
                Banned = true;
            }

            return Banned;
        }

        public void Ban(string FacebookId) 
        {
            if (isBanned(FacebookId) == false)
            {
                tblBan ban = new tblBan();
                
                ban.BanDate = DateTime.Now.Date;
                ban.BanDatetime = DateTime.Now;
                ban.FacebookId = FacebookId;

               // ent.Entry(ban).State = System.Data.EntityState.Added;

                ent.tblBans.Add(ban);

                ent.SaveChanges();
            }
        }

        public void Forgive(string FacebookId) 
        {
            if (isBanned(FacebookId) == true)
            {
                tblBan ban = new tblBan();

                ban = ent.tblBans.Where(f => f.FacebookId == FacebookId).FirstOrDefault();

                // ent.Entry(ban).State = System.Data.EntityState.Deleted;

                ent.tblBans.Remove(ban);

                ent.SaveChanges();
            }
        }


    }
}
