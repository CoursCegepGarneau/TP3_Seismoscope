using Seismoscope.Model;
using Seismoscope.Utils.Enums;

public abstract class User
{
    public int Id { get; set; } // Clé primaire

    public string ?Prenom { get; set; }

    public string ?Nom { get; set; }

    public string ?Username { get; set; }

    public string ?Password { get; set; }

    public abstract UserRole Role { get; }
}
