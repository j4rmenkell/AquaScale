using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.T_PM_Reservation — property ownership history per lot.
/// Logical PK: ReservationNo (char 8), configured as key in WebsDbContext with ValueGeneratedNever().
/// BackoutType IS NULL = reservation is active (buyer currently owns the lot).
/// AquaScale uses: ReservationNo, CompPBL, Buyer_ID, DateReserved, BackoutType, DateUpdated.
/// The full table has ~80 real-estate financial columns — only AquaScale-relevant ones are mapped.
/// Schema validated against WEBS-Schema.txt.
/// </summary>
public class WEBSReservation
{
    [MaxLength(8)]
    public string    ReservationNo { get; set; } = null!; // char(8) logical PK
    public DateTime? DateReserved  { get; set; }           // smalldatetime NULL
    [MaxLength(40)]
    public string    CompPBL       { get; set; } = null!; // varchar(40) NOT NULL — matches Property.CompPbl
    [MaxLength(2)]
    public string    Scheme_ID     { get; set; } = null!; // char(2) NOT NULL
    [MaxLength(10)]
    public string?   Bank_ID       { get; set; }           // char(10) NULL
    [MaxLength(8)]
    public string    BuyerId       { get; set; } = null!; // Buyer_ID char(8) NOT NULL — FK to M_Buyer.Buyer_ID
    [MaxLength(10)]
    public string?   Broker_ID     { get; set; }           // varchar(10) NULL
    [MaxLength(10)]
    public string?   Agent_ID      { get; set; }           // varchar(10) NULL
    [MaxLength(2)]
    public string?   BackoutType   { get; set; }           // char(2) NULL — NULL = active reservation
    public DateTime  DateUpdated   { get; set; }           // smalldatetime NOT NULL — always populated
    [MaxLength(2)]
    public string?   Status        { get; set; }           // char(2) NULL
    public DateTime? StatusDate    { get; set; }           // smalldatetime NULL
}
