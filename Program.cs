using System;
using System.Collections.Generic;

namespace Debts
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<XAccrualItem> accrualItems = new List<XAccrualItem> ();
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-10)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-9)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-5)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-4)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-3)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-2)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-1)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-8)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-7)));
            accrualItems.Add (new XAccrualItem (100, DateTime.Now.AddMonths (-6)));

            accrualItems.Add (new XAccrualItem (100, DateTime.Now));

            //Платежи сначало идут не в полном объеме, а потом 
            List<XPaymentItem> paymentItems = new List<XPaymentItem> ();
            paymentItems.Add (new XPaymentItem (50, DateTime.Now.AddMonths (-5).AddDays (10)));
           // paymentItems.Add (new XPaymentItem (2000, DateTime.Now.AddMonths (-4).AddDays (10)));

            paymentItems.Add (new XPaymentItem (200, DateTime.Now.AddMonths (-3).AddDays (10)));
            paymentItems.Add (new XPaymentItem (50, DateTime.Now.AddMonths (-2).AddDays (10)));
            paymentItems.Add (new XPaymentItem (50, DateTime.Now.AddMonths (-9).AddDays (10)));
            paymentItems.Add (new XPaymentItem (150, DateTime.Now.AddMonths (-8).AddDays (10)));
            paymentItems.Add (new XPaymentItem (50, DateTime.Now.AddMonths (-7).AddDays (10)));


            XProcessor processor = new XProcessor (accrualItems, paymentItems, DateTime.Now, 0);



            Console.WriteLine ("Платежи  ______________________");
            foreach (XPaymentItem pi in paymentItems)
                Console.WriteLine (pi.ToString ());



            Console.WriteLine ("Итоги  ______________________");
            foreach (XDebtItem di in processor.Debts)
            { 
                Console.WriteLine (di.ToString ());
            }

            Console.WriteLine ("Остаток сальдо после последнего погашения задолженности платежом: "+processor.CurrentSaldo.ToString("0.##"));

            Console.WriteLine ("Итоги для расчета пени ______");
            foreach (XDebtItem di in processor.PostProcess())
            {
                Console.WriteLine (di.ToString ());
            }

        }
    }



}
