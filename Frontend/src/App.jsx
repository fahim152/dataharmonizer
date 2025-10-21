import React, { useEffect, useState } from 'react';
import RequestList from './features/requests/RequestList';
import HarmonizeEditor from './features/harmonization/HarmonizeEditor';
import HarmonizedRecords from './features/records/HarmonizedRecords';
import RecordModal from './features/records/RecordModal';
import { fetchRequests, fetchRecords, harmonizeRequest } from './api/harmonizer';

const App = () => {
  const [requests, setRequests] = useState([]);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [records, setRecords] = useState([]);
  const [modalRecord, setModalRecord] = useState(null);

  useEffect(() => {
    fetchRequests().then(setRequests);
  }, []);

  useEffect(() => {
    if (selectedRequest) {
      fetchRecords(selectedRequest.id).then(setRecords);
    } else {
      setRecords([]);
    }
  }, [selectedRequest]);

  const handleHarmonize = async ({ contextPrompt, desiredSchema, outputType }) => {
    if (!selectedRequest) return;
    await harmonizeRequest(selectedRequest.id, contextPrompt, desiredSchema);
    fetchRecords(selectedRequest.id).then(setRecords);
  };

  return (
    <div style={{ display: 'flex', height: '100vh' }}>
      <RequestList
        requests={requests}
        selectedId={selectedRequest?.id}
        onSelect={setSelectedRequest}
      />
      <HarmonizeEditor
        request={selectedRequest}
        onHarmonize={handleHarmonize}
      />
      <HarmonizedRecords
        records={records}
        onSelect={setModalRecord}
      />
      {modalRecord && (
        <RecordModal record={modalRecord} onClose={() => setModalRecord(null)} />
      )}
    </div>
  );
};

export default App;

