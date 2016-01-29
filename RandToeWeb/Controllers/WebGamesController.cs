using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RandToeWeb.Models;

namespace RandToeWeb.Controllers
{
    public class WebGamesController : Controller
    {
        private RandToeWebContext db = new RandToeWebContext();

        // GET: WebGames
        public ActionResult Index()
        {
            return View(db.WebGames.ToList());
        }

        // GET: WebGames/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WebGame webGame = db.WebGames.Find(id);
            if (webGame == null)
            {
                return HttpNotFound();
            }
            return View(webGame);
        }

        // GET: WebGames/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: WebGames/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PublicId,IsOver")] WebGame webGame)
        {
            if (ModelState.IsValid)
            {
                db.WebGames.Add(webGame);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(webGame);
        }

        // GET: WebGames/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WebGame webGame = db.WebGames.Find(id);
            if (webGame == null)
            {
                return HttpNotFound();
            }
            return View(webGame);
        }

        // POST: WebGames/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PublicId,IsOver")] WebGame webGame)
        {
            if (ModelState.IsValid)
            {
                db.Entry(webGame).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(webGame);
        }

        // GET: WebGames/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WebGame webGame = db.WebGames.Find(id);
            if (webGame == null)
            {
                return HttpNotFound();
            }
            return View(webGame);
        }

        // POST: WebGames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            WebGame webGame = db.WebGames.Find(id);
            db.WebGames.Remove(webGame);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
