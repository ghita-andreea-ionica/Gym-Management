using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GymManagement
{
    public enum SubscriptionType
    {
        Lunar,
        Anual
    }

    public enum ClassType
    {
        Fitness,
        Pilates,
        Yoga
    }

    public class Trainer
    {
        public string Name { get; set; }
        public string Specialization { get; set; }

        public Trainer(string name, string specialization)
        {
            Name = name;
            Specialization = specialization;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"    Antrenor: {Name}");
            Console.WriteLine($"    Specializare: {Specialization}");
        }
    }

    public class ClassSession
    {
        public string Id { get; set; }
        public ClassType Type { get; set; }
        public Trainer Trainer { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxCapacity { get; set; }
        public List<string> Participants { get; set; }
        public DateTime StartTime { get; set; }
        public string RoomLocation { get; set; }

        public ClassSession(ClassType type, Trainer trainer, int durationMinutes, int maxCapacity, DateTime startTime,
            string roomLocation)
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8);
            Type = type;
            Trainer = trainer;
            DurationMinutes = durationMinutes;
            MaxCapacity = maxCapacity;
            Participants = new List<string>();
            StartTime = startTime;
            RoomLocation = roomLocation;
        }

        public bool CanReserve()
        {
            return Participants.Count < MaxCapacity;
        }

        public bool Reserve(string username)
        {
            if (Participants.Contains(username))
                return false;

            if (!CanReserve())
                return false;

            Participants.Add(username);
            return true;
        }

        public bool CancelReservation(string username)
        {
            return Participants.Remove(username);
        }

        public double GetOccupancyRate()
        {
            return MaxCapacity > 0 ? (double)Participants.Count / MaxCapacity * 100 : 0;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"\n  [{Id}] Clasă {Type}");
            Console.WriteLine($"    Data/Ora: {StartTime:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"    Durată: {DurationMinutes} minute");
            Console.WriteLine($"    Locație: {RoomLocation}");
            Trainer.DisplayInfo();
            Console.WriteLine($"    Locuri: {Participants.Count}/{MaxCapacity} ({GetOccupancyRate():F1}% ocupat)");
            Console.WriteLine($"    Status: {(CanReserve() ? "DISPONIBIL" : "COMPLET")}");
        }
    }

    public class Subscription
    {
        public SubscriptionType Type { get; set; }
        public decimal Price { get; set; }
        public List<string> Benefits { get; set; }
        public int DurationDays { get; set; }

        public Subscription(SubscriptionType type)
        {
            Type = type;
            Benefits = new List<string>();

            if (type == SubscriptionType.Lunar)
            {
                Price = 150;
                DurationDays = 30;
                Benefits.Add("Acces la toate zonele");
                Benefits.Add("1 sesiune de evaluare gratuită");
                Benefits.Add("Dus și vestiare");
            }
            else
            {
                Price = 1500;
                DurationDays = 365;
                Benefits.Add("Acces la toate zonele");
                Benefits.Add("2 sesiuni cu antrenor personal/lună");
                Benefits.Add("Dus și vestiare");
                Benefits.Add("Acces prioritar în orele de vârf");
            }
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"\n--- ABONAMENT {Type.ToString().ToUpper()} ---");
            Console.WriteLine($"Preț: {Price} RON");
            Console.WriteLine($"Durată: {DurationDays} zile");
            Console.WriteLine("Beneficii:");
            foreach (var benefit in Benefits)
            {
                Console.WriteLine($"  ✓ {benefit}");
            }
        }
    }

    public class Zone
    {
        public string Name { get; set; }
        public string Schedule { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentOccupancy { get; set; }

        public Zone(string name, string schedule, int maxCapacity)
        {
            Name = name;
            Schedule = schedule;
            MaxCapacity = maxCapacity;
            CurrentOccupancy = 0;
        }

        public bool CanEnter()
        {
            return CurrentOccupancy < MaxCapacity;
        }

        public bool CheckIn()
        {
            if (CanEnter())
            {
                CurrentOccupancy++;
                return true;
            }

            return false;
        }

        public void CheckOut()
        {
            if (CurrentOccupancy > 0)
                CurrentOccupancy--;
        }

        public double GetOccupancyRate()
        {
            return MaxCapacity > 0 ? (double)CurrentOccupancy / MaxCapacity * 100 : 0;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"\n{Name}");
            Console.WriteLine($"  Program: {Schedule}");
            Console.WriteLine($"  Capacitate: {CurrentOccupancy}/{MaxCapacity} ({GetOccupancyRate():F1}%)");
            Console.WriteLine($"  Status: {(CanEnter() ? "Disponibil" : "Complet")}");
        }
    }

    public class FitnessRoom
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Dictionary<string, Zone> Zones { get; set; }
        public List<ClassSession> Classes { get; set; }

        public FitnessRoom(string name, string address)
        {
            Name = name;
            Address = address;
            Zones = new Dictionary<string, Zone>();
            Classes = new List<ClassSession>();
            InitializeZones();
        }

        private void InitializeZones()
        {
            Zones.Add("Cardio", new Zone("Zona Cardio", "06:00 - 12:00", 25));
            Zones.Add("Forta", new Zone("Zona Forță", "12:00 - 19:00", 20));
            Zones.Add("Tractiuni", new Zone("Zona Tracțiuni", "18:00 - 23:00", 15));
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"  {Name}");
            Console.WriteLine($"  Adresă: {Address}");
            foreach (var zone in Zones.Values)
            {
                zone.DisplayInfo();
            }
        }

        public void DisplayClasses()
        {
            Console.WriteLine($"  CLASE DISPONIBILE - {Name}");
            if (Classes.Count == 0)
            {
                Console.WriteLine("  Nu există clase programate.");
                return;
            }

            foreach (var classSession in Classes.OrderBy(c => c.StartTime))
            {
                classSession.DisplayInfo();
            }
        }

        public Zone GetZone(string zoneName)
        {
            return Zones.ContainsKey(zoneName) ? Zones[zoneName] : null;
        }

        public ClassSession GetClass(string classId)
        {
            return Classes.FirstOrDefault(c => c.Id == classId);
        }

        public void AddClass(ClassSession classSession)
        {
            Classes.Add(classSession);
        }

        public bool RemoveClass(string classId)
        {
            var classToRemove = GetClass(classId);
            if (classToRemove != null)
            {
                Classes.Remove(classToRemove);
                return true;
            }

            return false;
        }
    }

    public abstract class User
    {
        private string _password;
        public string Username { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }

        protected User(string username, string password, string fullname)
        {
            Username = username;
            _password = HashPassword(password);
            FullName = fullname;
            CreatedAt = DateTime.Now;
        }

        protected User()
        {
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public bool VerifyPassword(string password)
        {
            return _password == HashPassword(password);
        }

        public (bool success, string message) ChangePassword(string oldPassword, string newPassword)
        {
            if (!VerifyPassword(oldPassword))
                return (false, "Parola veche incorectă!");

            _password = HashPassword(newPassword);
            return (true, "Parola a fost schimbată cu succes!");
        }

        public string GetPasswordHash() => _password;
        public void SetPasswordHash(string hash) => _password = hash;
        public abstract Dictionary<string, object> GetData();
        public abstract string GetUserType();
    }

    public class Client : User
    {
        public string MembershipStatus { get; set; }
        public DateTime? MembershipExpiry { get; set; }
        public SubscriptionType? CurrentSubscription { get; set; }
        public List<DateTime> Visits { get; set; }
        public string CurrentZone { get; set; }
        public string PreferredRoom { get; set; }
        public List<string> ReservedClasses { get; set; }

        public Client(string username, string password, string fullname)
            : base(username, password, fullname)
        {
            MembershipStatus = "inactiv";
            MembershipExpiry = null;
            CurrentSubscription = null;
            Visits = new List<DateTime>();
            CurrentZone = null;
            PreferredRoom = null;
            ReservedClasses = new List<string>();
        }

        public Client() : base()
        {
            Visits = new List<DateTime>();
            ReservedClasses = new List<string>();
        }

        public string ActivateMembership(SubscriptionType type)
        {
            var subscription = new Subscription(type);
            MembershipStatus = "activ";
            CurrentSubscription = type;
            MembershipExpiry = DateTime.Now.AddDays(subscription.DurationDays);

            return
                $"Abonament {type} activat!\nPreț: {subscription.Price} RON\nValabil până la: {MembershipExpiry:yyyy-MM-dd}";
        }

        public string AddVisit()
        {
            if (MembershipStatus != "activ")
                return "Abonamentul nu este activ!";

            if (MembershipExpiry < DateTime.Now)
            {
                MembershipStatus = "expirat";
                return "Abonamentul a expirat!";
            }

            DateTime visitDate = DateTime.Now;
            Visits.Add(visitDate);
            return $"Vizită înregistrată la {visitDate:yyyy-MM-dd HH:mm:ss}";
        }

        public string CheckInToZone(FitnessRoom room, string zoneName)
        {
            if (MembershipStatus != "activ")
                return "Nu ai un abonament activ!";

            if (CurrentZone != null)
                return $"Ești deja în {CurrentZone}. Te rog să ieși mai întâi.";

            var zone = room.GetZone(zoneName);
            if (zone == null)
                return "Zona nu există!";

            if (zone.CheckIn())
            {
                CurrentZone = zoneName;
                PreferredRoom = room.Name;
                return $"Check-in efectuat în {zone.Name} la {room.Name}!";
            }

            return $"{zone.Name} este complet. Capacitate: {zone.CurrentOccupancy}/{zone.MaxCapacity}";
        }

        public string CheckOutFromZone(FitnessRoom room)
        {
            if (CurrentZone == null)
                return "Nu ești în nicio zonă!";

            var zone = room.GetZone(CurrentZone);
            if (zone != null)
            {
                zone.CheckOut();
                string message = $"Check-out efectuat din {zone.Name}!";
                CurrentZone = null;
                return message;
            }

            return "Eroare la check-out!";
        }

        public override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "username", Username },
                { "fullname", FullName },
                { "membership_status", MembershipStatus },
                { "subscription_type", CurrentSubscription?.ToString() ?? "N/A" },
                { "membership_expiry", MembershipExpiry?.ToString("yyyy-MM-dd") ?? "N/A" },
                { "total_visits", Visits.Count },
                { "reserved_classes", ReservedClasses.Count },
                { "current_zone", CurrentZone ?? "N/A" },
                { "preferred_room", PreferredRoom ?? "N/A" },
                { "created_at", CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        }

        public override string GetUserType() => "Client";
    }
    public class Admin : User
    {
        public string AccessLevel { get; set; }
        public Admin(string username, string password, string fullname, string accessLevel = "standard")
            : base(username, password, fullname)
        {
            AccessLevel = accessLevel;
        }
        public Admin() : base() { }
        public override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "username", Username },
                { "fullname", FullName },
                { "access_level", AccessLevel },
                { "role", "Administrator" },
                { "created_at", CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        }
        public override string GetUserType() => "Admin";
    }
}