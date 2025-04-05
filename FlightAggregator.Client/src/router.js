import { createRouter, createWebHistory } from 'vue-router';
import Login from './components/Login.vue';
import Flights from './components/Flights.vue';

const routes = [
    {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/login',
      component: Login
    },
    {
      path: '/flights',
      component: Flights,
      meta: { requiresAuth: true }
    }
  ];

const router = createRouter({
    history: createWebHistory(),
    routes
});

router.beforeEach((to, from, next) => {
    const isAuthenticated = !!localStorage.getItem('token');
    
    if (to.meta.requiresAuth && !isAuthenticated) {
      next('/login');
    } else {
      next();
    }
  });

export default router;