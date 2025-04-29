namespace WorkWise.ViewModels
{
    public class SquadViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LeaderName { get; set; }
        public bool IsCurrentUserLeader { get; set; } // Добавляем флаг лидера
        public string UserRole => IsCurrentUserLeader ? "Лидер" : "Участник"; // Вычисляемое свойство
    }
}