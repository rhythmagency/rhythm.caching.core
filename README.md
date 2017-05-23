# Introduction

Provides mechanisms to cache and invalidate caches.

The two primary classes used to cache are InstanceCache and InstanceByKeyCache. The latter is essentially a dictionary version of the former.

Refer to the [generated documentation](docs/generated.md) for more details.

# Installation

Install with NuGet. Search for "Rhythm.Caching.Core".

# Overview

There are two types of caches:

* **InstanceCache** Stores an instance of a variable.
* **InstanceByKeyCache** Stores instances of variables in a dictionary.

There are also two interfaces, `ICacheInvalidator` and `ICacheByKeyInvalidator`, that
are implemented by other libraries. They can be used to facilitate invalidation of
caches (e.g., when events occur that update values stored by the caches).

In particular, the Rhythm.Caching.Umbraco library implements both of these interfaces
to invalidate caches when Umbraco content changes.

## InstanceCache

Typically, you will create an instance of an `InstanceCache` in a static variable.
Any time you need the variable stored by the InstanceCache, call the `Get` method,
passing to it a function that will retrieve the new value. This function will only
be called if the cache needs to be updated.

Here's an example that retrieves a random number from the cache. Because it is from
the cache, the random number only changes once an hour (the duration of the cache):

```c#
private static InstanceCache<int> NumberCache { get; set; } = new InstanceCache<int>();
private static Random Generator { get; set; } = new Random();
public int GetRandomNumberEveryHour()
{
    var duration = TimeSpan.FromHours(1);
    var number = NumberCache.Get(duration, () =>
    {
        return Generator.Next();
    });
    return number;
}
```

## InstanceByKeyCache

The InstanceByKeyCache is similar to the InstanceCache, except the InstanceByKeyCache
expects an addition parameter of a key in the `Get` method when fetching a value
from the cache.

# Maintainers

To create a new release to NuGet, see the [NuGet documentation](docs/nuget.md).
