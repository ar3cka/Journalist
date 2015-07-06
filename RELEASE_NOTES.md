### 0.6.9 (2015-07-06)
* [EventStore][BugFix] Event headers value is optional property for journaled event.

### 0.6.8 (2015-07-01)
* [LanguageExtensions][Breaking] StringExtensions.JoinStringsWithComma was renamed to ToCsvString.
* [LanguageExtensions][Breaking] Formatting methods were moved to StringFormattingExtensions class.
* [LanguageExtensions][New] EqualsCs and EqualsCi string comparison extension methods were added.
* [EventStrore][New] Add headers support to JournaledEvent.

### 0.6.7 (2015-06-30)
* [WAZ.Storage][BugFix] Fix argument exception on batch operation with delete action execution.
* [WAZ.Storage][New] Extensions methods for TableBatchOperation.

### 0.6.6 (2015-06-29)
* [EventStrore][Breaking] Fix bug in JournaledEvent.Create from dicitonary method.

### 0.6.5 (2015-06-29)
* [LanguageExtensions] SelectToArray and SelectToList extensions methods.
* [EventStrore][Breaking] JournaledEvent.EventPayload converted to method.

### 0.6.4 (2015-06-25)
* [EventStore][Improvment] Named event stream consumers.
* [EventStore][Breaking] IEventStreamCursor.Slice property returns interface IEventStreamSlice.

### 0.6.3 (2015-06-25)
* [WAZ.Storage][New] Extensions and new methods for CloudBlockBlob.
* [EventStore][Improvment] Endless consumer session break implemenation.

### 0.6.2 (2015-06-24)
* [WAZ.Storage][New] Set of extensions query methods for TableStorageQuery.
* [EventStore][Breaking] Interfaces for EventStreamCursor and EventStreamSlice.

### 0.6.1 (2015-06-24)
* [EventStore][BUGFIX] EventStreamConsumer.CloseAsync fail when current has been already commited.
* [EventStore][IMPROVEMENT] Exclusive stream access for event stream consumer group.

### 0.6.0 (2015-06-23)
* [EventStore][Breaking] High level API for producing and consuming streams.
* [EventStore][Breaking] EventStreamReader API was changed.
* [EventStore][Breaking] EventJournal API was changed.

### 0.5.0 (2015-06-19)
* [EventStore][Breaking] Rename EventStream to EventStoreConnection.
* [EventStore][Breaking] Serialization functions were removed.
* [EventStore][Breaking] Open* methods were renamed to Create*.
* [EventStore] Json.Net dependency was removed.

### 0.4.0 (2015-06-18)
* [WAZ.Storage] CloudQueue API.

### 0.3.1 (2015-06-15)
* [EventStoe] Options type serialization support was added in Journalist.EventStore.Streams.Serializers.Json.JsonEventSerializer.

### 0.3.0 (2015-06-11)
* [LanguageExtensions] Require.Positive and Require.ZeroOrGreater method for int type.
* [EventStore] OpenEventStream reader from specified stream version.

### 0.2.0 (2015-06-11)
* [LanguageExtensions] Require.NotEmpty method for collection was added.
* [LanguageExtensions] IsNullOrEmpty string extension method was added.
* [EventStore] EventStream implementation.
* [EventStore] Json event serialization support.

### 0.1.0 (Released 2015-06-09)
* Add azure storage block blob basic api.

### 0.0.2 (Released 2015-06-09)
* Made StorageFactory not static.

### 0.0.1 (Released 2015-06-08)
* Windows azure storage classes.
* EventJournal basic implementation.
* LanguageExtensions basic implementation.
