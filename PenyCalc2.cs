using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debts
{

    /// <summary>
    /// Выставленный счет, требование по уплате услуг (товара)
    /// </summary>
    public class XAccrualItem : IComparable<XAccrualItem>
    {
        public double Summa { get; set; } = 00.00;
        public DateTime MoratoriumDate { get; set; }

        public XAccrualItem(double Summa, DateTime MoratoriumDate)
        {
            this.Summa=Summa;
            this.MoratoriumDate=MoratoriumDate;
        }

        /// <summary>
        /// Метод сравнения для сортировки массивов/списков из элементов данного класс.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>1 - больше / -1 меньше</returns>
        public int CompareTo(XAccrualItem other)
        {
            return this.MoratoriumDate.Ticks>other.MoratoriumDate.Ticks ? 1 : (this.MoratoriumDate.Ticks==other.MoratoriumDate.Ticks ? 0 : -1);
        }

        /// <summary>
        /// Начисление в форме строки
        /// </summary>
        /// <returns></returns>
        public string ToString()
        {
            return MoratoriumDate.ToShortDateString ()+" - "+Summa.ToString ("0.##");
        }
    }

    /// <summary>
    /// Платеж за оказанную услугу
    /// </summary>
    public class XPaymentItem : IComparable<XPaymentItem>
    {
        public double Summa { get; set; } = 00.00;
        public DateTime PaymentDate { get; set; }

        public XPaymentItem(double Summa, DateTime PaymentDate)
        {
            this.Summa=Summa;
            this.PaymentDate=PaymentDate;
        }

        /// <summary>
        /// Функция сравнения: используется для сортировки массивов
        /// </summary>
        /// <param name="other">Стандартный параметр</param>
        /// <returns>-1/0/1 - меньше/равно/больше</returns>
        public int CompareTo(XPaymentItem other)
        {
            return this.PaymentDate.Ticks>other.PaymentDate.Ticks ? 1 : (this.PaymentDate.Ticks==other.PaymentDate.Ticks ? 0 : -1);
        }

        /// <summary>
        /// Платеж в форме строки
        /// </summary>
        /// <returns></returns>
        public string ToString()
        {
            return PaymentDate.ToShortDateString ()+" * "+Summa.ToString ("0.##");
        }
    }

    /// <summary>
    /// Класс долговой структуры
    /// </summary>
    public class XDebtItem
    {
        public double Summa { get; set; } = 00.00;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double Debt { get; set; } = 0.0;

        public double TotalDebt { get; set; } = 0.0;

        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Длительность существования долга
        /// </summary>
        public int Duration { get { return (new TimeSpan (EndDate.Ticks-StartDate.Ticks)).Days; } }

        public XDebtItem(double Summa, DateTime StartDate, DateTime EndDate)
        {
            this.Summa=Summa;
            this.StartDate=StartDate;
            this.EndDate=EndDate;
            this.Debt=Summa;
            this.TotalDebt=Summa;

        }
        /// <summary>
        /// Простой вывод долга в форме строки
        /// </summary>
        /// <returns></returns>
        public string ToString()
        {
            return (Summa.ToString ("0.00")+" "+StartDate.ToShortDateString ()+" - - - "+EndDate.ToShortDateString ()+"\t Долг конечный = "+TotalDebt.ToString ("###00.00")+$"\t Закрыто = {(IsCompleted ? "ДА" : "НЕТ")} \t Срок = {this.Duration} дней");
        }

    }

    public class XDebtProcessor
    {
        public List<XDebtItem> Debts = new List<XDebtItem> ();
        public double CurrentSaldo = 0.0;

        /// <summary>
        /// Обработчик задолженности: класс определяет фактическую задолженность по счетам/платежам. 
        /// </summary>
        /// <param name="accruals">Список задолженностей (счетов)</param>
        /// <param name="payments">Список платежей</param>
        /// <param name="TargetDate">Дата крайнего срока расчета</param>
        /// <param name="StartSaldo">Долг до начала расчета</param>
        public XDebtProcessor(List<XAccrualItem> accruals, List<XPaymentItem> payments, DateTime TargetDate, double StartSaldo = 0.0)
        {
            CurrentSaldo=StartSaldo;
            accruals.Sort();
            payments.Sort();

            //Загоняем в задолженности все начисления, до даты расчета
            foreach (XAccrualItem acc in accruals )
                if (acc.MoratoriumDate<TargetDate)
                    Debts.Add (new XDebtItem (acc.Summa, acc.MoratoriumDate, TargetDate));

            //Теперь всеми платежами по очереди считаем CurrentSaldo, если задолженность менее 0, то
            //можно выключать задолженность
            for (int i = 0; i<payments.Count; i++)
            {
                bool push_Debt = false;
                int j = -1;

                //Формируем начальный остаток - вычитаем платеж.
                CurrentSaldo=-payments[i].Summa;

                #if DEBUG
                Console.WriteLine ($"Платеж #{i} от {payments[i].PaymentDate.ToShortDateString ()}  -{payments[i].Summa}  -- остаток {CurrentSaldo.ToString ("0.##")}");
                Console.WriteLine ($"--- Расчет ----");
                #endif

                //Проходим по начислениям и 
                foreach (XDebtItem debtItem in Debts)
                {
                    j++;
                    //если долг закрыт, то идем к следующему
                    if (debtItem.IsCompleted)
                        continue;

                    //Долг по начислению в списке 
                    double tmp_debt = debtItem.Debt;

                    //Корректируем плавающий долг - выедаем платеж начислениями
                    #if DEBUG
                        Console.Write ($"  Начисление +{tmp_debt}р.  #{j} от {debtItem.StartDate.ToShortDateString ()} -- остаток {CurrentSaldo.ToString ("0.##")}");
                    #endif
                    //Корректируем общей сумму задоженности на сумму начисления
                    CurrentSaldo=CurrentSaldo+tmp_debt;

                    #if DEBUG
                        Console.WriteLine ($" -> {CurrentSaldo.ToString ("0")}");
                    #endif

                    //Платеж перекрыл начисление   - Долг закрыт?
                    if (CurrentSaldo<=0.0)  //Да, идем к следующей задолженности после
                    {
                        debtItem.IsCompleted=true; //больше не участвует в погашении долгов
                        push_Debt=true;
                        debtItem.EndDate=payments[i].PaymentDate; //отметка когда закрыта  задолженность
                        debtItem.TotalDebt=debtItem.Debt;               //Конечная сумма долга на момент закрытия
                        debtItem.Debt=0;                                //Погашено!
                    }
                    else //Сохраняем остаток долга в структуру, если платеж не погасил начисление
                    {
                        debtItem.Debt=CurrentSaldo;
                        debtItem.TotalDebt=debtItem.Debt;
                        break; //идем к другому платежу, может он погасит???!
                    }
                }  //Итак ходим и корректируем все начисления через платеж, откусывая задолженности платежами

                #if DEBUG
                    Console.WriteLine ($"--- Расчет закончен. ----");
                    foreach (XDebtItem di in Debts)
                        Console.WriteLine (di.ToString ());
                    Console.WriteLine ($"");
                #endif                
            } //i-следующий платеж

            //-- Обходим все задолженности ---
            foreach (XDebtItem di in Debts)
            {
                if (!di.IsCompleted)
                {
                    if (di.Debt==0.0)
                        di.IsCompleted=true; //нет долга- закрыт!
                    else
                        di.EndDate=TargetDate;//если не закрыт то - текущая дата
                }
            }
        }//Конструктор процессора

        /// <summary>
        /// Возвращает список задолженностей, например подлежащих уплате пени, в соответствии требования законодательства РФ - более 30 дней.
        /// Отбираются в т.ч. уже закрытые задолженности.
        /// </summary>
        /// <param name="OnlyOpened">Выбрать только непогашенные долги - True </param>
        /// <returns>Список задолженностей со сроками, датами начала и окончания существования задолженности.</returns>
        public List<XDebtItem> PostProcess(bool OnlyOpened)
        {
            return Debts.Where (x => x.Duration>30 && ((OnlyOpened && !x.IsCompleted) || !OnlyOpened) ).ToList ();
        }
    }
}
