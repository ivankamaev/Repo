//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class project
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public project()
        {
            this.project_equipment = new HashSet<project_equipment>();
        }
    
        public int projectID { get; set; }
        public Nullable<int> createrID { get; set; }
        public Nullable<System.DateTime> arrival { get; set; }
        public Nullable<System.DateTime> installation { get; set; }
        public Nullable<System.DateTime> rehearsal { get; set; }
        public Nullable<System.DateTime> start { get; set; }
        public Nullable<System.DateTime> finish { get; set; }
        public Nullable<System.DateTime> deinstallation { get; set; }
        public Nullable<System.DateTime> departure { get; set; }
        public Nullable<int> placeID { get; set; }
        public string worktype { get; set; }
        public Nullable<int> executorID { get; set; }
        public string type { get; set; }
        public Nullable<int> showmanID { get; set; }
        public Nullable<int> managerID { get; set; }
        public Nullable<int> clientID { get; set; }
        public string content { get; set; }
        public string note { get; set; }
        public Nullable<double> receipts_cash { get; set; }
        public Nullable<double> receipts_noncash { get; set; }
        public Nullable<double> expenditure_cash { get; set; }
        public Nullable<double> expenditure_noncash { get; set; }
        public Nullable<double> profit_cash { get; set; }
        public Nullable<double> profit_noncash { get; set; }
        public Nullable<double> profit_total { get; set; }
    
        public virtual contact contact { get; set; }
        public virtual contact contact1 { get; set; }
        public virtual contact contact2 { get; set; }
        public virtual contact contact3 { get; set; }
        public virtual place place { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<project_equipment> project_equipment { get; set; }
        public virtual user user { get; set; }
    }
}