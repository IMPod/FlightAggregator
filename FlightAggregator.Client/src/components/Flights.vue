<template>
    <div>
      <h1>Flights</h1>
      <input v-model="filters.airline" placeholder="Airline" />
      <input v-model.number="filters.minPrice" type="number" placeholder="Min Price" />
      <input v-model.number="filters.maxPrice" type="number" placeholder="Max Price" />
      <button @click="fetchFlights">Search</button>
      <ul>
        <li v-for="flight in flights" :key="flight.id">
          {{ flight.airline }} - {{ flight.price }} USD
          <router-link :to="`/book/${flight.id}`">Book</router-link>
        </li>
      </ul>
    </div>
  </template>
  
  <script setup>
  import { ref, reactive, onMounted } from 'vue';
  import api from '../api'; 
  
  const flights = ref([]);
  const filters = reactive({
    airline: '',
    minPrice: null,
    maxPrice: null
  });
  
  const fetchFlights = async () => {
    try {
      const params = Object.fromEntries(
        Object.entries(filters).filter(([_, v]) => v !== null && v !== '')
      );
      
      const response = await api.get('/aggregator/flights', { params });
      flights.value = response.data;
    } catch (error) {
      console.error('Error fetching flights:', error);
    }
  };
  
  onMounted(fetchFlights);
  </script>