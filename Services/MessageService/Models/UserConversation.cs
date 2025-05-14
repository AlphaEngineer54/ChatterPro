using System;
using System.Collections.Generic;

namespace MessageService.Models;

public partial class UserConversation
{
    public int UserId { get; set; }

    public int? ConversationId { get; set; }

    public int Id { get; set; }

    public virtual Conversation? Conversation { get; set; }
}
