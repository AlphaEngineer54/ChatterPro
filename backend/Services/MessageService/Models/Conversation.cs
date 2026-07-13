using System;
using System.Collections.Generic;

namespace MessageService.Models;

public partial class Conversation
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public DateTime Date { get; set; }

    public string JoinCode { get; set; } = null!;

    public int OwnerId { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<UserConversation> Users { get; set; } = new List<UserConversation>();
}
