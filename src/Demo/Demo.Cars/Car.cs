﻿using System.ComponentModel;
using System.Threading.Tasks;
using FluentValidation;
using Platformex;

namespace Demo.Cars
{
    #region hack

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit{}

    #endregion

//Идентификатор
    public class CarId : Identity<CarId>
    {
        public CarId(string value) : base(value) {}
    }
    
//Команды
    [Description("Создать вагон")]
    [Public]
    [Rules(typeof(CreateCarValidator))]
    public record CreateCar(CarId Id, string Name) : ICommand<CarId>;

    [Description("Переименовать вагон")]
    public record RenameCar(CarId Id, string NewName) : ICommand<CarId>;

    [Description("Удалить вагон")]
    public record DeleteCar(CarId Id) : ICommand<CarId>;

//Бизнес-правила
    public class CreateCarValidator : Rules<CreateCar>
    {
        public CreateCarValidator() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().WithMessage("Имя не может быть пустым");
        }
    }

//События
    public record CarCreated(CarId Id, string Name) : IAggregateEvent<CarId>;
    public record CarRenamed(CarId Id, string NewName, string OldName) : IAggregateEvent<CarId>;

//Интерфейс агрегата
    public interface ICar : IAggregate<CarId>,
        ICanDo<CreateCar, CarId>,
        ICanDo<RenameCar, CarId>,
        ICanDo<DeleteCar, CarId>
    {
        public Task<CommandResult> RenameCar(string newName) 
            => Do(new RenameCar(this.GetId<CarId>() , newName));
    }
}