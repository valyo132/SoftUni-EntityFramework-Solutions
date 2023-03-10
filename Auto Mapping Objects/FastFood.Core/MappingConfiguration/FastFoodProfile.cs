namespace FastFood.Core.MappingConfiguration
{
    using AutoMapper;
    using FastFood.Core.ViewModels.Categories;
    using FastFood.Core.ViewModels.Employees;
    using FastFood.Core.ViewModels.Items;
    using FastFood.Core.ViewModels.Orders;
    using FastFood.Models;
    using ViewModels.Positions;

    public class FastFoodProfile : Profile
    {
        public FastFoodProfile()
        {
            //Positions
            CreateMap<CreatePositionInputModel, Position>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.PositionName));

            CreateMap<Position, PositionsAllViewModel>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.Name));

            CreateMap<Position, RegisterEmployeeViewModel>()
                .ForMember(e => e.PositionId, p => p.MapFrom(opt => opt.Id));

            // Employees
            CreateMap<RegisterEmployeeInputModel, Employee>();

            CreateMap<Employee, EmployeesAllViewModel>()
                .ForMember(e => e.Position, ev => ev.MapFrom(opt => opt.Position.Name));

            // Categories
            CreateMap<CreateCategoryInputModel, Category>()
                .ForMember(ci => ci.Name, c => c.MapFrom(opt => opt.CategoryName));

            CreateMap<Category, CategoryAllViewModel>();

            // Items
            CreateMap<Category, CreateItemViewModel>()
                .ForMember(ci => ci.CategoryId, c => c.MapFrom(opt => opt.Id));

            CreateMap<CreateItemInputModel, Item>();

            CreateMap<Item, ItemsAllViewModels>()
                .ForMember(im => im.Category, i => i.MapFrom(opt => opt.Category.Name));

            // Orders
            CreateMap<CreateOrderInputModel, Order>();

            CreateMap<CreateOrderInputModel, OrderItem>();

            CreateMap<Order, OrderAllViewModel>()
                .ForMember(om => om.Employee, o => o.MapFrom(opt => opt.Employee.Name))
                .ForMember(om => om.DateTime, o => o.MapFrom(opt => opt.DateTime.ToString("dd/MM/yyyy")))
                .ForMember(om => om.OrderId, o => o.MapFrom(opt => opt.Id));
        }
    }
}
