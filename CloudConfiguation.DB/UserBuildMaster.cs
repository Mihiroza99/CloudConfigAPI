using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CloudConfiguation.DB
{
    [Table("UserBuildMaster")]
    public class UserBuildMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BuildId { get; set; }
        public int UserId { get; set; }
        public string SessionId { get; set; }
        public string Template { get; set; }
        public string BuildFile { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
