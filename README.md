# MediFlow

MediFlow is a Mediator Based Communication System

## Features
- Lightweight and efficient.
- Supports publish-subscribe, request-response, and one-way notifications.
- Simplifies working with decoupled services in distributed systems.
- Easy to integrate with existing applications.
- Supports commands and command handlers for request-response communication.

## Installation
You can install the MediFlow NuGet package using the following command:

```bash
dotnet add package MediFlow
```

## Basic implementation of MediFlow using clean architecture

```bash
https://github.com/italomangueira/CleanArchComMediFlow
```

## Usage

To reference only the contracts for SimpleMediator, which includes:

- `IRequest` (including generic variants)
  - Represents a command or query that expects a single response
- `INotification`
  - Represents an event broadcast to multiple handlers (if any)

### Advanced Usage: Request + Notification

This example demonstrates how to combine a `Request` (command/query) and a `Notification` (event) in a real-world use case.

> ✅ This scenario uses only `Microsoft.Extensions.DependencyInjection.Abstractions` for DI registration — no framework-specific packages.

```bash
Install-Package Microsoft.Extensions.DependencyInjection.Abstractions
```

---

#### 1. Define the Request and Notification

```csharp
public class CreateProductCommand : IRequest<string>
{
    public string Title { get; set; }
}

public class ProductCreatedEvent : INotification
{
    public Guid ProductId { get; }

    public ProductCreatedEvent(Guid productId)
    {
        ProductId = productId;
    }
}
```

---

#### 2. Implement the Handlers

```csharp
public class CreateProductHandler : IRequestHandler<CreateProductCommand, string>
{
    private readonly IMediFlow _mediator;

    public CreateProductHandler(IMediFlow mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        // Simulate persistence...

        // Publish event
        await _mediator.Publish(new ProductCreatedEvent(id), cancellationToken);

        return $"Product '{request.Title}' created with ID {id}";
    }
}

public class SendWelcomeEmailHandler : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending welcome email to customer {notification.ProductId}");
        return Task.CompletedTask;
    }
}
```

---

#### 3. Register the Handlers (Dependency Injection)

You can register everything manually if you want full control:

```csharp
services.AddSingleton<IMediFlow, MediFlow>();

services.AddTransient<IRequestHandler<CreateProductCommand, string>, CreateProductHandler>();
services.AddTransient<INotificationHandler<ProductCreatedEvent>, SendWelcomeEmailHandler>();
```

Or use assembly scanning with:

```csharp
builder.Services.AddMediFlow();
```

---

#### 4. Execute the Flow

```csharp
public class ProductAppService
{
    private readonly IMediFlow _mediator;

    public CustomerAppService(IMediFlow mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> CreateProduct(string title)
    {
        return await _mediator.Send(new CreateProductCommand { Title = title });
    }
}
```

---

When the `CreateProduct` method is called:

1. `CreateProductHandler` handles the request
2. It creates and persists the customer (simulated)
3. It publishes a `ProductCreatedEvent`
4. `SendWelcomeEmailHandler` handles the event

This structure cleanly separates **commands** (which change state and return a result) from **notifications** (which communicate to the rest of the system that something happened).


## License
This project is licensed under the MIT License. See the LICENSE file for details.
