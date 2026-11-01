using System.Text;

/// <summary>
/// The digits typed on the participant keypad (#27). Pure C#: it can only ever produce
/// "P" + 3 digits, so the keypad cannot send an invalid ID to the logger.
/// </summary>
public sealed class ParticipantCodeBuffer
{
    private const int CodeLength = 3;

    private readonly int[] digits = new int[CodeLength];

    public int Length { get; private set; }

    public bool IsComplete => Length == CodeLength;

    /// <summary>"P017" once three digits are typed, otherwise null.</summary>
    public string Code
    {
        get
        {
            if (!IsComplete)
            {
                return null;
            }

            return "P" + digits[0] + digits[1] + digits[2];
        }
    }

    /// <summary>Spaced for the keypad display, empty places as underscores: "P 0 1 _".</summary>
    public string Display
    {
        get
        {
            var text = new StringBuilder("P");
            for (int i = 0; i < CodeLength; i++)
            {
                text.Append(' ').Append(i < Length ? (char)('0' + digits[i]) : '_');
            }

            return text.ToString();
        }
    }

    /// <summary>Returns false (and changes nothing) for a non-digit or when the code is already full.</summary>
    public bool AddDigit(int digit)
    {
        if (digit < 0 || digit > 9 || IsComplete)
        {
            return false;
        }

        digits[Length++] = digit;
        return true;
    }

    public bool DeleteLast()
    {
        if (Length == 0)
        {
            return false;
        }

        Length--;
        return true;
    }

    public void Clear()
    {
        Length = 0;
    }
}
