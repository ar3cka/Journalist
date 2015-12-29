### 0.9.1
* [EventStore][Breaking] Event stream readers classes were removed.
* [WAZ.Storage][New] CloudTable GetAll queries.

### 0.9.0
* [EventStore][Breaking] Move StreamVersion to the Events namespace.
* [EventStore][Breaking] Rename EventStreamPosition to EventStreamHeader.
* [EventStore][Improvment] Partitioned notificaion channel.
* [EventStore][Improvment] Processing unpublished stream updatenotifications.

### 0.8.5
* [EventStore][Improvment] Caching data in EventJournalReaders and EventStreamConsumers.
* [WAZ.Storage][BugFix] Support for Boolean, DateTimeOffset properties in table storage.

### 0.8.4
* [EventStore][New] EventJournal support reader streams.
* [WAZ.Storage][New] CloudQueue getting messages operation accept visibility timeout parameter.
* [WAZ.Storage][New] CloudQueue getting messages operation accept message count parameter.
* [WAZ.Storage][New] CloudTable expose and accept table continuation token.
* [EventStore][Improvment] Consumer can start reading from the end of streams.
* [EventStore][Improvment] Retry notification delivery timeout.
* [EventStore][Improvment] Journal commit reader version methods skip update when version are equal.
* [EventStore][Breaking] Notification listener starts reading stream from the end.
* [EventStore][Breaking] EventJournal.ReadStreamReaderPositionAsync throw exception, if reader is not registered.
* [EventStore][Breaking] Consumer creations methods with EventStreamConsumerId parameter were removed.
* [EventStore][Breaking] Notifications address by listener type instead of consumer identifier.
* [EventStore][Breaking] Journal commit reader version method throws when passed version is less then existing.
* [EventStore][BugFix] Consumer initialize reader only in leader state.

### 0.8.3
* [EventStore][New] Stream position property in stream readers and writers.

### 0.8.2
* Fixed nuget package

### 0.8.1
* [EventStore][Improvment] Limit number of notification processing.
* [LanguageExtensions][New] ToInvariantString extension method for TimeSpan, DateTime, DateTimeOffset.

### 0.8.0
* [EventStore][New] Stream changes notificaion listneres.
* [LanguageExtensions][New] IsEmpty extensions methods for collections.
* [LanguageExtensions][New] Ensure helper methods.
* [EventStore][Breaking] EventStoreConnection related were moved to separate namespace.
* [EventStore][Breaking] API changes in journal namespace.
* [EventStore][Breaking] EventStream cosnumers name reader cursor based on its unqiue identifier.
* [EventStore][Breaking] StreamVersion.Parse accepts "0" string.

### 0.7.4 (2015-07-21)
* [WAZ.Storage][Improvment] Log storage request information from StorageException in TableBatchOperationAdapter.

### 0.7.3 (2015-07-08)
* [LanguageExtensions][BugFix] EmptyMemoryStream.Get() returns reusable (non disposable) stream instance.

### 0.7.2 (2015-07-08)
* [WAZ.Storage][Breaking] TableEntity queries return specific types instead of interfaces.

### 0.7.1 (2015-07-08)
* [LanguageExtensions][New] SelectToArray and SelectToList for enumerables of KeyValuePair<,>.
* [WAZ.Storage][New] Batch operation methods and table queries with partition key only.

### 0.7.0 (2015-07-07)
* [EventStore][Breaking] EventStoreConnectionBuilder was moved from EventStore.Configuration to EventStore namespace.
* [EventStore][Breaking] EventJournal saves event headers as byte array.
* [EventStore][Breaking] Suffix Async was added to EventStoreConnection.CreateStreamProducer and CreateStreamConsumer.
* [EventStore][New] Event stream events mutation pipelines implemented.
* [LanguageExtensions][New] EmptyMemoryStream helper class was added.

### 0.6.9 (2015-07-06)
* [EventStore][BugFix] Event headers value is optional property for journaled event.

### 0.6.8 (2015-07-01)
* [LanguageExtensions][Breaking] StringExtensions.JoinStringsWithComma was renamed to ToCsvString.
* [LanguageExtensions][Breaking] Formatting methods were moved to StringFormattingExtensions class.
* [LanguageExtensions][New] EqualsCs and EqualsCi string comparison extension methods were added.
* [EventStore][New] Add headers support to JournaledEvent.

### 0.6.7 (2015-06-30)
* [WAZ.Storage][BugFix] Fix argument exception on batch operation with delete action execution.
* [WAZ.Storage][New] Extensions methods for TableBatchOperation.

### 0.6.6 (2015-06-29)
* [EventStrore][Breaking] Fix bug in JournaledEvent.Create from dicitonary method.

### 0.6.5 (2015-06-29)
* [LanguageExtensions] SelectToArray and SelectToList extensions methods.
* [EventStore][Breaking] JournaledEvent.EventPayload converted to method.

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
