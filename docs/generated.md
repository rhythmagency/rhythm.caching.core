# Rhythm.Caching.Core

<table>
<tbody>
<tr>
<td><a href="#instancebykeycache\`2">InstanceByKeyCache\`2</a></td>
<td><a href="#instancecache\`1">InstanceCache\`1</a></td>
</tr>
<tr>
<td><a href="#stringarraycomparer">StringArrayComparer</a></td>
<td><a href="#cachegetmethod">CacheGetMethod</a></td>
</tr>
<tr>
<td><a href="#icachebykeyinvalidator">ICacheByKeyInvalidator</a></td>
</tr>
</tbody>
</table>


## InstanceByKeyCache\`2

Caches instance variables by key in a dictionary-like structure.

#### Type Parameters

- T - The type of value to cache.
- TKey - The key to use when access values in the dictionary.

### Constructor

Default constructor.

### Clear

Clears the cache.

### ClearKeys(keys)

Clears the cache of the specified keys.

| Name | Description |
| ---- | ----------- |
| keys | *System.Collections.Generic.IEnumerable{\`1}*<br>The keys to clear the cache of. |

### Get(key, replenisher, duration, method, keys)

Gets the instance variable (either from the cache or from the specified function).

| Name | Description |
| ---- | ----------- |
| key | *\`1*<br>The key to use when fetching the variable. |
| replenisher | *System.Func{\`1,\`0}*<br>The function that replenishes the cache. |
| duration | *System.TimeSpan*<br>The duration to cache for. |
| method | *Rhythm.Caching.Core.Enums.CacheGetMethod*<br>Optional. The cache method to use when retrieving the value. |
| keys | *System.String[]*<br>Optional. The keys to store/retrieve a value by. Each key combination will be treated as a separate cache. |

#### Returns

The value.

### Instances

The instances stored by their key, then again by a contextual key.

### InstancesLock

Object to perform locks for cross-thread safety.

### TryGetByKeys(keys, accessKey)

Trys to get the value by the specified keys.

| Name | Description |
| ---- | ----------- |
| keys | *System.String[]*<br>The keys. |
| accessKey | *\`1*<br>The key to use to access the value. |

#### Returns

The value, or the default for the type.

### UpdateValueByKeys(keys, accessKey, value, lastCache, doLock)

Updates the cache value by the specified keys.

| Name | Description |
| ---- | ----------- |
| keys | *System.String[]*<br>The keys to cache by. |
| accessKey | *\`1*<br>The key to use to access the value. |
| value | *\`0*<br>The value to update the cache with. |
| lastCache | *System.DateTime*<br>The date/time to mark the cache as last updated. |
| doLock | *System.Boolean*<br>Lock the instance cache during the update? |

### UpdateValueByKeysWithoutLock(keys, accessKey, value)

Updates the cache with the specified value.

| Name | Description |
| ---- | ----------- |
| keys | *System.String[]*<br>The keys to cache by. |
| accessKey | *\`1*<br>The key to use to access the value. |
| value | *\`0*<br>The value to update the cache with. |


## InstanceCache\`1

Caches an instance variable.

#### Type Parameters

- T - The type of variable to cache.

### Constructor

Default constructor.

### Clear

Clears the cache.

### Get(duration, replenisher, method, keys)

Gets the instance variable (either from the cache or from the specified function).

| Name | Description |
| ---- | ----------- |
| duration | *System.TimeSpan*<br>The duration to cache for. |
| replenisher | *System.Func{\`0}*<br>The function that replenishes the cache. |
| method | *Rhythm.Caching.Core.Enums.CacheGetMethod*<br>Optional. The cache method to use when retrieving the value. |
| keys | *System.String[]*<br>Optional. The keys to store/retrieve a value by. Each key combination will be treated as a separate cache. |

#### Returns

The value.

### TryGetByKeys(keys)

Trys to get the value by the specified keys.

| Name | Description |
| ---- | ----------- |
| keys | *System.String[]*<br>The keys. |

#### Returns

The value, or the default for the type.

### UpdateValueByKeys(keys, value, doLock)

Updates the cache value by the specified keys.

| Name | Description |
| ---- | ----------- |
| keys | *System.String[]*<br>The keys to cache by. |
| value | *\`0*<br>The value to update the cache with. |
| doLock | *System.Boolean*<br>Lock the instance cache during the update? |


## StringArrayComparer

Compares an array of strings.

### Equals(x, y)

Check if the arrays are equal.

| Name | Description |
| ---- | ----------- |
| x | *System.String[]*<br>The first array. |
| y | *System.String[]*<br>The second array. |

#### Returns

True, if the arrays are both null, are both empty, or both have the same strings in the same order; otherwise, false.

### GetHashCode(items)

Generates a hash code by combining all of the hash codes for the strings in the array.

| Name | Description |
| ---- | ----------- |
| items | *System.String[]*<br>The array of strings. |

#### Returns

The combined hash code.


## CacheGetMethod

The methods that can be used when retrieving values from a cache.

### Default

Gets a value from the cache, replenishing the cache with a new value if necessary.

### FromCache

Gets the cached value, or null. Does not modify the cached value or call the replenisher.

### NoCache

Gets a new value each time and does not store it to the cache.

### NoStore

Gets a value from the cache, or gets a new value, but will not store the new value to the cache.

### Recache

Gets a new value and stores the result to the cache.


## ICacheByKeyInvalidator

Interface to be used by invalidators that apply to keyed caches.

### InvalidateForKeys(keys)

Invalidates the cache for the specified keys.

| Name | Description |
| ---- | ----------- |
| keys | *System.Collections.Generic.IEnumerable{System.Object}*<br>The keys to invalidate. |
