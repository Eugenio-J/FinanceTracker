import { useState, useEffect } from 'react';
import { useAuthStore } from '@/stores/auth-store';

export const useHydration = () => {
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    // Check if store is already hydrated, else wait for it
    const unsubHydrate = useAuthStore.persist.onHydrate(() => setHydrated(false));
    const unsubFinishHydration = useAuthStore.persist.onFinishHydration(() => setHydrated(true));
    
    setHydrated(useAuthStore.persist.hasHydrated());

    return () => {
      unsubHydrate();
      unsubFinishHydration();
    };
  }, []);

  return hydrated;
};
