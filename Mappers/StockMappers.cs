using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Stock;
using api.Models;

namespace api.Mappers
{
    public static class StockMappers
    {
        public static StockDto ToStockDto(this Stock StockModel)
        {
            return new StockDto
            {
                Id = StockModel.Id,
                Symbol = StockModel.Symbol,
                CompanyName = StockModel.CompanyName,
                Purchase = StockModel.Purchase,
                Industry = StockModel.Industry,
                MarketCap = StockModel.MarketCap,
                LastDiv = StockModel.LastDiv,
                //CreatedOn = StockModel.CreatedOn
            };
        }
        
    }
}