using System;
using System.Collections.Generic;

namespace MessageService.Models;

public partial class Message
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Date { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public int? ConversationId { get; set; }

    public string Status { get; set; } = null!;

    public virtual Conversation? Conversation { get; set; }
}
