import { useEffect, useMemo, useState } from "react";
import "../styles/tokens.css";
import { fetchLots, LOT_STATUS } from "../data/mockLots";
import FieldWorkerHeader from "../components/layout/FieldWorkerHeader";
import ScreenFooter from "../components/layout/ScreenFooter";
import LotTabs from "../components/ui/LotTabs";
import SearchFilterBar from "../components/ui/SearchFilterBar";
import LotCard from "../components/ui/LotCard";
import MeterTypeModal from "../components/ui/MeterTypeModal";
import "./Lots.css";

// Swap this for real values from your auth/location context once wired up.
const MOCK_SESSION = {
  subdivision: "Tanza A",
  inBounds: true,
};

function matchesUtilityFilter(lot, filterValue) {
  if (filterValue === "all") return true;
  if (filterValue === "water+electricity") {
    return lot.utilities.includes("water") && lot.utilities.includes("electricity");
  }
  return lot.utilities.includes(filterValue);
}

function matchesSearch(lot, query) {
  if (!query.trim()) return true;
  const haystack = `blk ${lot.block} lot ${lot.lotNumber}`.toLowerCase();
  return haystack.includes(query.trim().toLowerCase());
}

export default function Lots() {
  const [allLots, setAllLots] = useState([]);
  const [activeTab, setActiveTab] = useState("pending");
  const [searchValue, setSearchValue] = useState("");
  const [filterValue, setFilterValue] = useState("all");
  const [meterModalLot, setMeterModalLot] = useState(null);

  useEffect(() => {
    let isMounted = true;
    fetchLots().then((lots) => {
      if (isMounted) setAllLots(lots);
    });
    return () => {
      isMounted = false;
    };
  }, []);

  const pendingLots = useMemo(
    () => allLots.filter((lot) => lot.status !== LOT_STATUS.CAPTURED),
    [allLots]
  );
  const capturedLots = useMemo(
    () => allLots.filter((lot) => lot.status === LOT_STATUS.CAPTURED),
    [allLots]
  );

  const visibleLots = useMemo(() => {
    const source = activeTab === "pending" ? pendingLots : capturedLots;
    return source
      .filter((lot) => matchesUtilityFilter(lot, filterValue))
      .filter((lot) => matchesSearch(lot, searchValue));
  }, [activeTab, pendingLots, capturedLots, filterValue, searchValue]);

  function handleLotPress(lot) {
    setMeterModalLot(lot);
  }

  function handleSelectMeterType(lot, utility) {
    // Hook this up to navigation, e.g.:
    // navigate(`/capture/${lot.id}?meter=${utility}`)
    console.log("Open camera for lot", lot.id, "meter:", utility);
    setMeterModalLot(null);
  }

  return (
    <div className="lots-screen">
      <FieldWorkerHeader
        subdivision={MOCK_SESSION.subdivision}
        inBounds={MOCK_SESSION.inBounds}
      />

      <LotTabs
        activeTab={activeTab}
        onChange={setActiveTab}
        pendingCount={pendingLots.length}
        capturedCount={capturedLots.length}
      />

      <SearchFilterBar
        searchValue={searchValue}
        onSearchChange={setSearchValue}
        filterValue={filterValue}
        onFilterChange={setFilterValue}
      />

      <div className="lots-screen__list" role="list">
        {visibleLots.length === 0 ? (
          <p className="lots-screen__empty">No lots match your search or filter.</p>
        ) : (
          visibleLots.map((lot) => (
            <LotCard key={lot.id} lot={lot} onPress={() => handleLotPress(lot)} />
          ))
        )}
      </div>

      <ScreenFooter message="Tap any job to open the camera." />

      <MeterTypeModal
        lot={meterModalLot}
        onSelectMeterType={handleSelectMeterType}
        onClose={() => setMeterModalLot(null)}
      />
    </div>
  );
}