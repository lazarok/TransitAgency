using AutoMapper;
using TransitAgency.Application.Features.Products.Commands.CreateProduct;
using TransitAgency.Application.Features.Products.Queries.GetAllProducts;
using TransitAgency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace TransitAgency.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<Product, GetAllProductsViewModel>().ReverseMap();
            CreateMap<CreateProductCommand, Product>();
            CreateMap<GetAllProductsQuery, GetAllProductsParameter>();
        }
    }
}
