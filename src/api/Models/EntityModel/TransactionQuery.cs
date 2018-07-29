﻿using System;
using System.Linq;

namespace api.Models.EntityModel
{
    public static class TransactionQuery
    {
        #region OrderByDescendingCreatedAt
        public static IQueryable<Transaction> OrderByDescendingCreatedAt(this IQueryable<Transaction> transactions)
        {
            return transactions.OrderByDescending(transaction => transaction.CreatedAt);
        }
        #endregion

        #region WhereDateFrom
        public static IQueryable<Transaction> WhereDateFrom(this IQueryable<Transaction> transactions, DateTime startDate)
        {
            return transactions.Where(transaction => transaction.CreatedAt.Date >= startDate.Date);
        }
        #endregion

        #region WhereAvailableAnticipation
        public static IQueryable<Transaction> WhereAvailableAnticipation(this IQueryable<Transaction> transactions)
        {
            return transactions.Where(transaction =>
                transaction.AcquirerConfirmation == true
                && transaction.TransferDate == null
                && transaction.TransferAmount == null)
                .Select(t => new Transaction
                {
                    Id = t.Id,
                    AcquirerConfirmation = t.AcquirerConfirmation,
                    CreatedAt = t.CreatedAt,
                    Installments = t.Installments,
                    TransferDate = t.TransferDate,
                    TransactionAmount = t.TransactionAmount,
                    TransferAmount = t.TransactionAmount - ((((decimal) 3.8 / 100) * (t.TransactionAmount/ t.Installments)) * t.Installments)
                });
        }
        #endregion

        #region WhereDateUntil
        public static IQueryable<Transaction> WhereDateUntil(this IQueryable<Transaction> transactions, DateTime endDate)
        {
            return transactions.Where(transaction => transaction.CreatedAt.Date <= endDate.Date);
        }
        #endregion
    }
}
