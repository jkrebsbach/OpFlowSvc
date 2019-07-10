using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Card : IBindableEntity
    {
        public int CardID { get; set; }
        public int LocationID { get; set; }
        public int ProviderID { get; set; }
        public int SurgeryID { get; set; }
        public int SurgeonID { get; set; }
        public int? ProcedureID { get; set; }
        public int? BundleID { get; set; }
        public int? CardCategoryID { get; set; }
        public int SpecialtyID { get; set; }
        public int OwnerUserID { get; set; }
        public int TemplateFlowID { get; set; }
        public int TemplateRoomSetupID { get; set; }
        public string CptCode { get; set; }
        public string SpecialtyDefaultFlag { get; set; }
        public string LocationName { get; set; }
        public string OwnerFullName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerFirstName { get; set; }
        public int FlowID { get; set; }
        public int RoomConfigID { get; set; }
        public int TimesUsed { get; set; }
        public long TimesUsedBig { get; set; }
        public string ProcedureDescription { get; set; }
        public string FlowDescription { get; set; }
        public string CardDescription { get; set; }
        public string BundleDescription { get; set; }
        public string SpecialtyDescription { get; set; }
        public decimal Cost { get; set; }


        public List<CardSource> Sources { get; set; }

        public int GetID()
        {
            return CardID;
        }

        public override string ToString()
        {
            return CardDescription;
        }
    }

    public class CardDetail : Card
    {
        public List<SurgeryUser> CardUsers { get; set; }
        public List<CardItem> CardItems { get; set; }
        public List<SurgeryInstrumentCount> SurgeryAdditionalItems { get; set; }
        public List<Procedure> CardProcedures { get; set; }
    }

    public class SurgeryCard : Card
    {
        public bool CurrentCard { get; set; }    
        public decimal CurrentCostDelta { get; set; }
    }

    public class CardQuantityEditRequest
    {
        public string Target { get; set; }
        public List<CardQuantityEdit> EditData { get; set; }
    }

    public class CardQuantityEdit
    {
        public int? TrayID { get; set; }
        public int ItemID { get; set; }
        public int? OpenQty { get; set; }
        public string HoldQty { get; set; }
        public bool DeleteItem { get; set; }
    }

    public class CardPost
    {
        public string Description { get; set; }
        public int OwnerUserID { get; set; }
        public int? SpecialtyID { get; set; }
        public int? ProcedureID { get; set; }
        public int? BundleID { get; set; }
        public string BundleFlag { get; set; }
        public string DefaultFlag { get; set; }
        public string SpecialtyDefaultFlag { get; set; }
        public string CardCategory { get; set; }
        public int? TemplateFlowID { get; set; }
        public int? TemplateRoomSetupID { get; set; }
        public List<CardPostImportProcedure> Procedures { get; set; }
    }

    public class CardPostImportProcedure
    {
        public string ImportSurgeon { get; set; }
        public string ImportProcedure { get; set; }
    }

    public class CardItemPost
    {
        public int OpenQty { get; set; }
        public int HoldQty { get; set; }
    }
}
