using Core.Domain;

namespace Core.Abstractions;

/// <summary>
/// Контракт сховища замовлень — лише ті операції, що справді потрібні сервісу.
/// Жодної специфіки файлу (SaveToFile/Flush) тут немає: контракт доменний, а не технічний.
/// IReadOnlyList — щоб зовнішній код не мутував сховище в обхід Add/Update/Remove.
/// </summary>
public interface IOrderStore
{
    IReadOnlyList<Order> List();
    Order? GetById(string id);      // Order? — запису з таким ключем може не бути
    void Add(Order item);
    void Update(Order item);
    bool Remove(string id);
}
