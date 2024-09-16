using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using easygames_final.Models;

namespace easygames_final.Controllers
{
    public class TransactionsController : Controller
    {
        private easygames_assessment1Entities db = new easygames_assessment1Entities();

        // GET: Transactions
        public ActionResult Index()
        {
            var transactions = db.Transactions.Include(t => t.Client).Include(t => t.TransactionType);
            return View(transactions.ToList());
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            ViewBag.ClientID = new SelectList(db.Clients, "ClientID", "Name");
            ViewBag.TransactionTypeID = new SelectList(db.TransactionTypes, "TransactionTypeID", "TransactionTypeName");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TransactionID,Amount,TransactionTypeID,ClientID,Comment")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var client = db.Clients.Find(transaction.ClientID);

                if (client != null)
                {
                    if (transaction.IsDebit)
                    {
                        client.ClientBalance += transaction.Amount;
                    }
                    else if (transaction.IsCredit)
                    {
                        client.ClientBalance -= transaction.Amount;
                    }

                    db.Transactions.Add(transaction);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(db.Clients, "ClientID", "Name", transaction.ClientID);
            ViewBag.TransactionTypeID = new SelectList(db.TransactionTypes, "TransactionTypeID", "TransactionTypeName", transaction.TransactionTypeID);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClientID = new SelectList(db.Clients, "ClientID", "Name", transaction.ClientID);
            ViewBag.TransactionTypeID = new SelectList(db.TransactionTypes, "TransactionTypeID", "TransactionTypeName", transaction.TransactionTypeID);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TransactionID,Amount,TransactionTypeID,ClientID,Comment")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var existingTransaction = db.Transactions.Find(transaction.TransactionID);
                var client = db.Clients.Find(transaction.ClientID);

                if (existingTransaction != null && client != null)
                {
                    // Reverse the old transaction
                    if (existingTransaction.IsDebit)
                    {
                        client.ClientBalance -= existingTransaction.Amount;
                    }
                    else if (existingTransaction.IsCredit)
                    {
                        client.ClientBalance += existingTransaction.Amount;
                    }

                    // Apply the new transaction
                    if (transaction.IsDebit)
                    {
                        client.ClientBalance += transaction.Amount;
                    }
                    else if (transaction.IsCredit)
                    {
                        client.ClientBalance -= transaction.Amount;
                    }

                    db.Entry(existingTransaction).CurrentValues.SetValues(transaction);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(db.Clients, "ClientID", "Name", transaction.ClientID);
            ViewBag.TransactionTypeID = new SelectList(db.TransactionTypes, "TransactionTypeID", "TransactionTypeName", transaction.TransactionTypeID);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var transaction = db.Transactions.Find(id);
            var client = db.Clients.Find(transaction.ClientID);

            if (transaction != null && client != null)
            {
                // Reverse the transaction impact on client balance
                if (transaction.IsDebit)
                {
                    client.ClientBalance -= transaction.Amount;
                }
                else if (transaction.IsCredit)
                {
                    client.ClientBalance += transaction.Amount;
                }

                db.Transactions.Remove(transaction);
                db.SaveChanges();
            }

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
