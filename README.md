# RBush .Net

Rbush это библиотека двухмерного **пространственного индекса** для точек и прямоугольников. Основано на оптимизированном R-tree с поддержкой вставок группой.

*Пространственный индекс* - это специальная структура данных для точек и прямоугольников, которая позволяет вам осуществлять запросы вида "все объекты в заданном прямоугольнике" очень эффективно, в сотни раз быстрее чем перебор всех элементов. Это часто используется в картах и при визуализации данных.

### Использование
## Создание

```
var tree = new RBush();
```
Дополнительный аргумент определяет максимальное количество элементов в узле. По-умолчанию 9, большое значение повышает скорость вставки, но замедляет поиск.
```
var tree = new RBush(16);
```
Если одно из полей класса реализует интейфейс IBBox
```
class BigData
{
    public int Id;
    public BBox Point;
}

var treeBigdata = new RBush<BigData>(bd => bd.Point);
```
## Добавление данных
```
var arrData = new List<int[]>()
{
    new int[] { 10, 50 },
    new int[] { 90, 50 },
    new int[] { 20, 30 }
};
var tree = new RBush();
for (int r = 0; r < arrData.Count; r++)
{
    tree.Insert(new BBox(arrData[r]));
}
```
## Удаление
Удаление поэлементно

```
tree.Remove(myItem);
```

Удаление всех объектов попавших в BBox
```
var filterBbox = new BBox(10, 20, 40, 30);
tree.Remove(filterBbox, e => true);
```
Либо в `Predicate` фильтруете какие объекты, попавшие в BBox удалить.

Очистить всё
```
tree.Clear();
```
## Вставка группой
```
var data = new List<BBox>(arrData.Count);
for (int r = 0; r < arrData.Count; r++)
{
    data.Add(new BBox(arrData[r]));
}
var tree = new RBush();
tree.AddRange(data.Cast<IBBox>());
```
Вставка группой в 2 раза быстрее, чем по одному. Последующие запросы, при вставке у пустое дерево, так же на 20% быстрее.
Имейте ввиду, что когда вы добавляете группу уже в существующее дерево, то оно загружает новые данные в отдельное дерево и вставляет меньшее дерево в большее. Поэтому групповая ставка хороша, когда вставляемые данные кластеризованы (т.е. близки друг к другу), но ухудшает производительность, когда данные разбросаны.
## Поиск
```
var bboxSearch = new BBox(20, 40, 100, 70);
var founds = tree.Search(bboxSearch);
```
Ограничивающая рамка задаётся как ```new BBox(minX, minY, maxX, maxY)```
## Коллизия
```
var result = tree.Collides(new BBox(20, 40, 100, 70));
```
Возвращает ```true``` если хотя бы один элемент пересекается с заданным прямоугольником. В противном случае ```false```.
## Авторы

* **Vladimir Agafonkin** - [Разработка под JavaScript](https://github.com/mourner/rbush)
* **freeExec** - [Реализация под .Net](https://github.com/freeExec/rbush.net)


## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/freeExec/rbush.net/blob/master/MIT-LICENSE) file for details
