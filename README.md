# MediFlow

MediFlow is a Mediator Based Communication System

## Features
- Lightweight and efficient.
- Supports publish-subscribe, request-response, and one-way notifications.
- Simplifies working with decoupled services in distributed systems.
- Easy to integrate with existing applications.
- Supports commands and command handlers for request-response communication.

## Installation
You can install the LightMediator NuGet package using the following command:

```bash
dotnet add package MediFlow
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
public class CreateCustomerCommand : IRequest<string>
{
    public string Title { get; set; }
}

public class CustomerCreatedEvent : INotification
{
    public Guid CustomerId { get; }

    public CustomerCreatedEvent(Guid customerId)
    {
        CustomerId = customerId;
    }
}
```

---

#### 2. Implement the Handlers

```csharp
public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, string>
{
    private readonly IMediFlow _mediator;

    public CreateCustomerHandler(IMediFlow mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        // Simulate persistence...

        // Publish event
        await _mediator.Publish(new CustomerCreatedEvent(id), cancellationToken);

        return $"Customer '{request.Title}' created with ID {id}";
    }
}

public class SendWelcomeEmailHandler : INotificationHandler<CustomerCreatedEvent>
{
    public Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending welcome email to customer {notification.CustomerId}");
        return Task.CompletedTask;
    }
}
```

---

#### 3. Register the Handlers (Dependency Injection)

You can register everything manually if you want full control:

```csharp
services.AddSingleton<IMediFlow, Mediflow>();

services.AddTransient<IRequestHandler<CreateCustomerCommand, string>, CreateCustomerHandler>();
services.AddTransient<INotificationHandler<CustomerCreatedEvent>, SendWelcomeEmailHandler>();
```

Or use assembly scanning with:

```csharp
services.AddSimpleMediator();
```

---

#### 4. Execute the Flow

```csharp
public class CustomerAppService
{
    private readonly IMediFlow _mediator;

    public CustomerAppService(IMediFlow mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> CreateCustomer(string title)
    {
        return await _mediator.Send(new CreateCustomerCommand { Title = title });
    }
}
```

---

When the `CreateCustomer` method is called:

1. `CreateCustomerHandler` handles the request
2. It creates and persists the customer (simulated)
3. It publishes a `CustomerCreatedEvent`
4. `SendWelcomeEmailHandler` handles the event

This structure cleanly separates **commands** (which change state and return a result) from **notifications** (which communicate to the rest of the system that something happened).


## License
This project is licensed under the MIT License. See the LICENSE file for details.
