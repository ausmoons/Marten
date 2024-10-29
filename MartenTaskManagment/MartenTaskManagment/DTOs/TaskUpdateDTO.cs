﻿namespace MartenTaskManagment.DTOs
{
    public class TaskUpdateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
    }
}
