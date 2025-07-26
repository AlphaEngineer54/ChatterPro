using MessageService.Interfaces;

namespace MessageService.Services
{
    public class JoinCodeConversationService : IGenerator<string>
    {
        public string Generate(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new();
            char[] joinCode = new char[length];
            int groupIndex = 4;

            for (int i = 0; i < length; i++)
            {
                groupIndex = groupIndex == 4 ? 0 : ++groupIndex;
                joinCode[i] = (groupIndex == 4)
                               ? '-'
                              : chars[random.Next(chars.Length)];

            }

            // Remove the last hyphen if it exists
            if (joinCode.Length > 0 && joinCode[^1] == '-')
            {
                Array.Resize(ref joinCode, joinCode.Length - 1);
            }

            return new string(joinCode); 
        }
    }
}
