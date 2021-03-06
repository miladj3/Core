﻿using System.ComponentModel.DataAnnotations;

namespace Core.Common.Dto.Api
{
    public class PaginationDto
    {
        [Required]
        public int PageIndex { get; set; }

        [Required]
        public int PageSize { get; set; } = 10;
    }
}
